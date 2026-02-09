# Violeta & Cacau – Ecossistema de Geração Automatizada de Vídeos

Geração automatizada de vídeos para TikTok/Reels voltada a marketing de afiliados e à marca **Violeta & Cacau** (doces).

## Estrutura do repositório

- **backend/** – .NET 9 Web API (Clean Architecture), preparada para Google Cloud Run
- **frontend/** – Dashboard Flutter (upload de imagem e acompanhamento do processamento)

## Arquitetura

1. **Backend** recebe uma imagem e orquestra:
   - **Vertex AI (Gemini 1.5 Flash)** – geração do roteiro a partir da imagem
   - **ElevenLabs** – síntese de voz (incluindo `voice_id` para voz clonada)
   - **Shotstack** – renderização do vídeo via JSON (timeline com imagem, áudio e texto)

2. **Frontend** permite enviar uma imagem e consultar o status do job (polling) até o vídeo ficar pronto.

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Flutter SDK](https://flutter.dev/docs/get-started/install)
- Contas e chaves:
  - **Google Cloud** – projeto com Vertex AI (Gemini) habilitado e Application Default Credentials (ou service account)
  - **ElevenLabs** – API Key e Voice ID (ou voz clonada)
  - **Shotstack** – API Key (ambiente `stage` para testes ou `v1` para produção)

## Configuração do Backend

1. Entre na pasta do backend:
   ```bash
   cd backend
   ```

2. Configure as chaves em `src/Posts.Api/appsettings.json` ou via variáveis de ambiente / User Secrets (não commite chaves):

   | Configuração | appsettings / Variável | Descrição |
   |--------------|------------------------|-----------|
   | Vertex AI | `VertexAI:ProjectId`, `VertexAI:Location`, `VertexAI:GeminiModel` | Projeto e região GCP; modelo (ex.: `gemini-1.5-flash`) |
   | ElevenLabs | `ElevenLabs:ApiKey`, `ElevenLabs:VoiceId`, `ElevenLabs:ModelId` | API Key, ID da voz, modelo (ex.: `eleven_multilingual_v2`) |
   | Shotstack | `Shotstack:ApiKey`, `Shotstack:Environment` | API Key; `stage` ou `v1` |
   | AssetStorage | `AssetStorage:AssetsBaseUrl` | URL base do backend (para a Shotstack acessar imagem/áudio; ex.: `http://localhost:5000` em dev) |

   Exemplo com User Secrets (desenvolvimento):
   ```bash
   cd src/Posts.Api
   dotnet user-secrets set "VertexAI:ProjectId" "seu-projeto-gcp"
   dotnet user-secrets set "ElevenLabs:ApiKey" "sua-chave"
   dotnet user-secrets set "ElevenLabs:VoiceId" "seu-voice-id"
   dotnet user-secrets set "Shotstack:ApiKey" "sua-chave-shotstack"
   ```

3. Execute a API:
   ```bash
   dotnet run --project src/Posts.Api
   ```
   A API sobe em `http://localhost:5000` (ou na porta indicada). Swagger: `http://localhost:5000/swagger`.

## Configuração do Frontend

1. Entre na pasta do frontend:
   ```bash
   cd frontend
   ```

2. Instale dependências:
   ```bash
   flutter pub get
   ```

3. Ajuste a URL da API em `lib/config/api_config.dart` se não for `http://localhost:5000` (ou use um pacote como `flutter_dotenv` e o arquivo `.env.example` como referência):
   ```dart
   const String kBackendBaseUrl = 'http://localhost:5000';
   ```

4. Execute o app:
   ```bash
   flutter run
   ```

## Docker e Google Cloud Run

- **Build da imagem** (a partir da raiz do repositório, com contexto em `backend`):
  ```bash
  cd backend
  docker build -f src/Posts.Api/Dockerfile -t posts-api .
  ```

- **Execução local**:
  ```bash
  docker run -p 8080:8080 -e VertexAI__ProjectId=... -e ElevenLabs__ApiKey=... -e ElevenLabs__VoiceId=... -e Shotstack__ApiKey=... posts-api
  ```

- **Cloud Run**: faça o deploy da imagem no Google Cloud Run e defina as variáveis de ambiente acima (e `AssetStorage__AssetsBaseUrl` com a URL pública da API). O Cloud Run define a variável `PORT`; a aplicação já usa `ASPNETCORE_URLS=http://+:8080`.

## Endpoints da API

- `POST /api/video/generate` – envia imagem (multipart/form-data, campo `image`) e opcionalmente `theme` e `voiceId`. Retorna `jobId` e mensagem.
- `GET /api/video/{jobId}/status` – retorna status do job (Pending, ScriptGenerated, AudioGenerated, RenderSubmitted, Done, Failed) e `videoUrl` quando concluído.
- `GET /api/assets/{fileName}` – serve assets (imagem/áudio) para a Shotstack.

## Licença

Uso interno / conforme definido pelo projeto.
