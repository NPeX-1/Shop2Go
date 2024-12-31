from fastapi import FastAPI, UploadFile, File, Form
import os
import uuid
from model import train_model, predict, DATAFOLDER
import uvicorn

app = FastAPI()

NUMADDED_FILE = "/shared_volume/numadded.txt"

def get_numadded():
    if os.path.exists(NUMADDED_FILE):
        with open(NUMADDED_FILE, "r") as f:
            return int(f.read().strip())
    return 0

def update_numadded(value):
    with open(NUMADDED_FILE, "w") as f:
        f.write(str(value))

@app.post("/train")
def train():
    try:
        train_model()
        return {"status": "success", "message": "Training completed successfully."}
    except Exception as e:
        return {"status": "error", "message": str(e)}

@app.post("/recognize")
async def recognize(file: UploadFile = File(...)):
    try:
        temp_file = f"temp_{file.filename}"
        with open(temp_file, "wb") as f:
            f.write(await file.read())

        type_name, model_name = predict(temp_file)

        os.remove(temp_file)

        return {"type": type_name, "model": model_name}
    except Exception as e:
        return {"status": "error", "message": str(e)}

@app.post("/add-image")
async def add_image(file: UploadFile = File(...), folder: str = Form(...), subfolder: str = Form(...)):
    try:
        if not (file.content_type in ["image/png", "image/jpeg"]):
            return {"status": "error", "message": "Unsupported file type. Only PNG and JPG are allowed."}

        folder_path = os.path.join(DATAFOLDER, folder.replace(" ", "_").upper())
        os.makedirs(folder_path, exist_ok=True)

        subfolder_path = os.path.join(folder_path, subfolder.replace(" ", "_"))
        os.makedirs(subfolder_path, exist_ok=True)

        file_name = f"{uuid.uuid4()}.{file.filename.split('.')[-1]}"
        file_path = os.path.join(subfolder_path, file_name)

        with open(file_path, "wb") as f:
            f.write(await file.read())

        numadded = get_numadded() + 1
        update_numadded(numadded)

        if numadded >= 50:
            train_model()
            update_numadded(0)

        return {"status": "success", "message": "File uploaded successfully.", "file_path": file_path}
    except Exception as e:
        return {"status": "error", "message": str(e)}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
