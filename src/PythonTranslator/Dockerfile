# Dockerfile para serviço Python de tradução
FROM python:3.11-slim

WORKDIR /app

# Instalar dependências do sistema
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# Copiar requirements e instalar dependências Python
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copiar código fonte
COPY . .

# Expor porta 5001
EXPOSE 5001

# Variáveis de ambiente (serão sobrescritas pelo docker-compose)
ENV FLASK_APP=app.py
ENV FLASK_ENV=production
ENV TRANSLATOR_PROVIDER=gemini
ENV TRANSLATOR_API_KEY=""

# Comando para rodar a aplicação
CMD ["python", "-m", "flask", "run", "--host=0.0.0.0", "--port=5001"]
