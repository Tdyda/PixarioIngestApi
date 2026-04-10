FROM python:3.12-slim

ENV DEBIAN_FRONTEND=noninteractive \
    PIP_NO_CACHE_DIR=1 \
    PYTHONUNBUFFERED=1 \
    COMFYUI_DIR=/opt/ComfyUI

WORKDIR /opt

RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    git-lfs \
    wget \
    curl \
    ffmpeg \
    libgl1 \
    libglib2.0-0 \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# ComfyUI
RUN git clone https://github.com/comfy-org/ComfyUI.git ${COMFYUI_DIR}

WORKDIR ${COMFYUI_DIR}

# Stable PyTorch z CUDA 12.6
RUN pip install --upgrade pip setuptools wheel && \
    pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu126

# Zależności ComfyUI
RUN pip install -r requirements.txt

# ComfyUI-Manager
RUN git clone https://github.com/ltdrdata/ComfyUI-Manager ${COMFYUI_DIR}/custom_nodes/ComfyUI-Manager

# Część node'ów i manager lubią mieć te pakiety dostępne
RUN pip install -r ${COMFYUI_DIR}/custom_nodes/ComfyUI-Manager/requirements.txt || true

EXPOSE 8188

CMD ["sh", "-c", "python main.py ${CLI_ARGS}"]
