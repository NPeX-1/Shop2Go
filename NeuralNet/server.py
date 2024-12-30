from fastapi import FastAPI, UploadFile, File
import os
from model import train_model, predict
import uvicorn

app = FastAPI()

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

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
