import os

import torch
import torch.nn as nn
from torchvision import models, datasets, transforms
from torch.utils.data import DataLoader, random_split, Dataset
from PIL import Image

from server import DATAFOLDER

EPOCHS = 75
#DATAFOLDER = "dataset"

class CustomDataset(Dataset):
    def __init__(self, root, transform=None):

        self.root_dir = root
        self.transform = transform

        # Initialize mappings and list of images
        self.type_to_idx = {}
        self.model_to_idx = {}
        self.img_to_model = {}
        self.img_to_type = {}
        self.img_paths = []

        numImgs = 0

        # Loop through the top-level folders (types) and their subfolders (models)
        for type_folder in os.listdir(root):
            type_path = os.path.join(root, type_folder)

            if os.path.isdir(type_path):  # Only consider directories (types)
                if type_folder not in self.type_to_idx:
                    self.type_to_idx[type_folder] = len(self.type_to_idx)

                # Get the index of the type
                type_idx = self.type_to_idx[type_folder]

                # Loop through the subfolders (models)
                for model_folder in os.listdir(type_path):
                    model_path = os.path.join(type_path, model_folder)

                    if os.path.isdir(model_path):  # Only consider subdirectories (models)
                        if model_folder not in self.model_to_idx:
                            self.model_to_idx[model_folder] = len(self.model_to_idx)

                        # Get the index of the model
                        model_idx = self.model_to_idx[model_folder]

                        for image_file in os.listdir(model_path):
                            image_path = os.path.join(model_path, image_file)
                            if os.path.isfile(image_path):
                                # Add the image path and corresponding model index to the list
                                self.img_paths.append(image_path)
                                self.img_to_model[numImgs] = model_idx
                                self.img_to_type[numImgs] = type_idx
                                numImgs += 1

        self.num_types = len(self.type_to_idx)
        self.num_models = len(self.model_to_idx)
        self.classes = {'types': list(self.type_to_idx.keys()), 'models': list(self.model_to_idx.keys())}

    def __len__(self):
        return len(self.img_paths)

    def __getitem__(self, idx):
        # Load image
        img_path = self.img_paths[idx]
        image = Image.open(img_path).convert("RGB")

        # Apply transformation if any
        if self.transform:
            image = self.transform(image)

        # Get the model index for this image
        model_idx = self.img_to_model[idx]

        # Extract the type index (based on model's type folder)
        type_idx = self.img_to_type[idx]

        return image, type_idx, model_idx


class MultiLevelClassifier(nn.Module):
    def __init__(self, num_types, num_models):
        super(MultiLevelClassifier, self).__init__()
        base_model = models.resnet50()
        self.feature_extractor = nn.Sequential(*list(base_model.children())[:-1])
        self.type_classifier = nn.Linear(2048, num_types)
        self.model_classifier = nn.Linear(2048, num_models)

    def forward(self, x):
        features = self.feature_extractor(x)
        features = features.view(features.size(0), -1)  # Flatten the features
        component_type = self.type_classifier(features)
        component_model = self.model_classifier(features)
        return component_type, component_model


transform = transforms.Compose([
        transforms.Resize((224, 224)),
        transforms.RandomHorizontalFlip(),
        transforms.ColorJitter(brightness=0.2, contrast=0.2, saturation=0.2, hue=0.1),
        transforms.ToTensor()
    ])

full_dataset = CustomDataset(root=DATAFOLDER, transform=transform)

train_size = int(0.9 * len(full_dataset))
val_size = len(full_dataset) - train_size
train_dataset, test_dataset = random_split(full_dataset, [train_size, val_size])

train_loader = DataLoader(train_dataset, batch_size=32, shuffle=True)
test_loader = DataLoader(test_dataset, batch_size=32, shuffle=False)

model = MultiLevelClassifier(num_types=full_dataset.num_types, num_models=full_dataset.num_models)
device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")

def train_model():
    model = MultiLevelClassifier(num_types=full_dataset.num_types, num_models=full_dataset.num_models)
    criterion = nn.CrossEntropyLoss()
    optimizer = torch.optim.Adam(model.parameters(), lr=1e-4)


    model = model.to(device)

    for epoch in range(EPOCHS):
        # Training Phase
        model.train()
        epoch_loss = 0
        for images, type_labels, model_labels in train_loader:
            images = images.to(device)
            type_labels = type_labels.to(device)
            model_labels = model_labels.to(device)

            # Forward pass
            type_pred, model_pred = model(images)

            # Compute losses
            loss_type = criterion(type_pred, type_labels)
            loss_model = criterion(model_pred, model_labels)
            loss = loss_type + loss_model

            # Backward pass
            optimizer.zero_grad()
            loss.backward()
            optimizer.step()

            epoch_loss += loss.item()

        print(f"Epoch {epoch + 1}, Loss: {epoch_loss / len(train_loader)}")

        # Validation Phase
        model.eval()
        correct_types = 0
        correct_models = 0
        total = 0

        with torch.no_grad():
            for images, type_labels, model_labels in test_loader:
                images = images.to(device)
                type_labels = type_labels.to(device)
                model_labels = model_labels.to(device)

                type_pred, model_pred = model(images)

                _, type_pred_labels = torch.max(type_pred, 1)
                _, model_pred_labels = torch.max(model_pred, 1)

                correct_types += (type_pred_labels == type_labels).sum().item()
                correct_models += (model_pred_labels == model_labels).sum().item()
                total += type_labels.size(0) + model_labels.size(0)

        type_accuracy = correct_types / total * 100
        model_accuracy = correct_models / total * 100
        print(f"Validation - Type Accuracy: {type_accuracy:.2f}%, Model Accuracy: {model_accuracy:.2f}%")

    # Save the trained model
    torch.save(model.state_dict(), "model.pth")
    print("Model saved to model.pth")

# 6. Inference
# Updated predict function
def predict(image_path):
    model = MultiLevelClassifier(num_types=full_dataset.num_types, num_models=full_dataset.num_models)
    model.load_state_dict(torch.load("model.pth", map_location=device))
    model = model.to(device)
    model.eval()

    # Preprocess the image
    image = Image.open(image_path).convert('RGB')
    image = transform(image).unsqueeze(0).to(device)

    # Perform prediction
    with torch.no_grad():
        type_pred, model_pred = model(image)
        _, type_pred_label = torch.max(type_pred, 1)
        _, model_pred_label = torch.max(model_pred, 1)

    type_name = full_dataset.classes['types'][type_pred_label.item()]
    model_name = full_dataset.classes['models'][model_pred_label.item()]

    return type_name, model_name

