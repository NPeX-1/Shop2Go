#!/bin/bash

if [ ! -d "$SHARED_VOLUME_PATH/dataset" ]; then
  echo "Dataset not found in shared volume. Copying..."
  cp -r /app/dataset $SHARED_VOLUME_PATH/
else
  echo "Dataset already exists in shared volume. Skipping copy."
fi

python server.py
