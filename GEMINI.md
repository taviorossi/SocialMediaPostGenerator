# Violeta & Cacau – Social Media Post Generator

Projeto de geração automatizada de vídeos (TikTok/Reels) a partir de imagens de produtos, focado na marca **Violeta & Cacau**.

## 🚀 Arquitetura e Fluxo

O sistema segue o padrão **Clean Architecture** no Backend e um dashboard simples em **Flutter** no Frontend.

### Fluxo de Geração de Vídeo:
1.  **Upload:** O Frontend envia uma imagem e tema para a API.
2.  **Roteiro:** O `GeminiService` (Vertex AI) analisa a imagem e gera um script persuasivo.
3.  **Voz:** O `ElevenLabsService` converte o script em áudio usando vozes clonadas ou pré-definidas.
4.  **Renderização:** O `ShotstackService` combina imagem, áudio e legendas em uma timeline e solicita a renderização do vídeo (`.mp4`).
5.  **Acompanhamento:** O Frontend realiza polling no status do `VideoJob` até que a URL final do vídeo esteja disponível.

## 🛠️ Tecnologias Principais

### Backend (.NET 9)
- **Framework:** ASP.NET Core Web API.
- **Orquestração:** `VideoOrchestratorService` gerencia o pipeline assíncrono.
- **IA Generativa:** Google Vertex AI (Gemini 1.5 Flash).
- **TTS:** ElevenLabs API.
- **Edição de Vídeo:** Shotstack API.
- **Persistência Temporária:** `VideoJobStore` (In-memory para este MVP).

### Frontend (Flutter)
- **Estado:** Polling para atualização de status de jobs.
- **Comunicação:** `VideoApiService` para integração com o backend.

## 📂 Estrutura de Pastas Relevante

```text
/Backend/src/
  ├── Posts.Api/            # Controllers e Configurações (Program.cs)
  ├── Posts.Application/    # Casos de uso e Interfaces (VideoOrchestratorService.cs)
  ├── Posts.Domain/         # Entidades e Enums (VideoJobStatus.cs)
  └── Posts.Infrastructure/ # Integrações com APIs externas (Gemini, ElevenLabs, Shotstack)

/Frontend/lib/
  ├── services/             # Lógica de integração com API
  ├── models/               # DTOs para comunicação com backend
  └── screens/              # UI (HomeScreen)
```

## 📝 Notas de Desenvolvimento

- **Endpoints Principais:**
  - `POST /api/video/generate`: Inicia o processo.
  - `GET /api/video/{jobId}/status`: Consulta o progresso.
  - `GET /api/assets/{fileName}`: Serve assets temporários para a Shotstack.
- **Configurações:** As chaves de API devem ser configuradas via `appsettings.json` ou Environment Variables (VertexAI, ElevenLabs, Shotstack).
- **Timeline:** Atualmente, a Shotstack é configurada com imagem estática (15s) e áudio de narração.

## ✅ Próximos Passos / Melhorias Sugeridas
- Implementar persistência real (SQL/NoSQL) para os Jobs.
- Adicionar suporte a múltiplos clipes/transições na timeline da Shotstack.
- Melhorar o tratamento de erros e retentativas em caso de falha nas APIs externas.
- Adicionar preview do roteiro gerado antes de iniciar a narração/renderização.
