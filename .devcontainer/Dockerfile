ARG VARIANT="buster"
FROM mcr.microsoft.com/vscode/devcontainers/base:0-${VARIANT}

RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg \
    && echo "deb [arch=amd64] https://packages.microsoft.com/debian/$(lsb_release -rs | cut -d'.' -f 1)/prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list \
    && apt-get update && apt-get install -y --no-install-recommends \
        powershell \
    && apt-get clean


ENV LIGHTSERVER_CONFIG "/workspaces/home.pi.lightserver/configs/lightserver.console.json"