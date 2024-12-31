FROM python:3.9-slim

WORKDIR /app

COPY . /app
RUN pip install --no-cache-dir -r requirements.txt

VOLUME ["/shared_volume"]

ENV SHARED_VOLUME_PATH=/shared_volume

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

CMD ["/entrypoint.sh"]
