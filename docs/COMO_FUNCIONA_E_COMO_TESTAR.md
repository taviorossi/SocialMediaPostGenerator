# Como o aplicativo funciona e como testar

## Visão geral

O sistema gera vídeos curtos (estilo TikTok/Reels) a partir de **uma imagem**: a API recebe a imagem, gera um roteiro com IA (Gemini), converte o roteiro em áudio (ElevenLabs) e manda montar o vídeo na Shotstack. Tudo isso roda em **background**; o cliente só envia a imagem e depois consulta o **status** até o vídeo ficar pronto.

---

## O que acontece passo a passo

### 1. Você envia uma imagem (app ou API)

- **App Flutter:** você toca em "Enviar imagem", escolhe uma foto (JPEG/PNG/WebP) e o app envia em `multipart/form-data` para a API.
- **API:** o endpoint `POST /api/video/generate` recebe o arquivo (campo `image`) e opcionalmente `theme` e `voiceId`.

### 2. A API responde na hora (assíncrono)

A API **não espera** o vídeo ficar pronto. Ela:

1. Cria um **job** com um `jobId` (GUID).
2. Guarda o job no store em memória (status inicial: **Pending**).
3. Inicia o pipeline em **background** (em outra tarefa).
4. Devolve para você algo assim:

```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": 0,
  "message": "Job submetido. Consulte o status em GET /api/video/{jobId}/status"
}
```

- **jobId:** use esse valor para consultar o status.
- **status:** número do enum (0 = Pending, 1 = ScriptGenerated, 2 = AudioGenerated, 3 = RenderSubmitted, 4 = Done, 5 = Failed).
- **message:** texto fixo explicando que é preciso consultar o status.

### 3. O que roda em background (pipeline)

Para aquele `jobId`, a API executa em sequência:

| Etapa | O que faz | Status no job |
|-------|-----------|----------------|
| 1 | Salva a imagem em disco e obtém uma URL (para a Shotstack usar depois) | continua Pending |
| 2 | Chama o **Gemini** (Google AI Studio) com a imagem + prompt (Violeta & Cacau, afiliados, até 30s) e recebe o **texto do roteiro** | **ScriptGenerated** |
| 3 | Envia o roteiro para o **ElevenLabs** e recebe o áudio (MP3); salva o áudio e obtém URL | **AudioGenerated** |
| 4 | Monta o JSON da timeline (imagem + áudio + título com trecho do roteiro) e envia para a **Shotstack** (render) | **RenderSubmitted** |
| 5 | Consulta o status do render na Shotstack uma vez; se vier `done` e tiver URL, atualiza o job | **Done** (ou **Failed** se der erro) |

Se qualquer etapa der exceção, o job vai para **Failed** e o campo `errorMessage` é preenchido.

### 4. Consulta de status

O cliente (ou você manualmente) chama:

**GET** `/api/video/{jobId}/status`

Exemplo de resposta enquanto ainda está processando:

```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": 2,
  "videoUrl": null,
  "errorMessage": null,
  "scriptGenerated": true,
  "audioGenerated": true,
  "renderSubmitted": false
}
```

Quando o vídeo estiver pronto (Shotstack retornar `done`):

```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": 4,
  "videoUrl": "https://shotstack-api-v1-output.s3-.../....mp4",
  "errorMessage": null,
  "scriptGenerated": true,
  "audioGenerated": true,
  "renderSubmitted": true
}
```

Se der erro em alguma etapa:

```json
{
  "jobId": "...",
  "status": 5,
  "videoUrl": null,
  "errorMessage": "Mensagem do erro (ex.: falha ao submeter render)",
  "scriptGenerated": true,
  "audioGenerated": false,
  "renderSubmitted": false
}
```

### 5. Comportamento do app Flutter

- Você toca em **"Enviar imagem"** → abre o seletor de arquivo → escolhe uma imagem → o app chama `POST /api/video/generate` e guarda o `jobId` da resposta.
- O app inicia um **polling** a cada 2 segundos em `GET /api/video/{jobId}/status`.
- O **card de status** mostra: Job ID, etapa atual (Aguardando upload, Gerando roteiro, Gerando áudio, Renderizando, Concluído, Erro) e, se houver, a mensagem de erro.
- Quando o status for **Done**, aparece a **URL do vídeo** e um botão **"Abrir vídeo"** (abre no navegador ou no app padrão).
- Quando for **Done** ou **Failed**, o polling para.

A URL da API no app está em `Frontend/lib/config/api_config.dart` (`kBackendBaseUrl`). Em desenvolvimento costuma ser `http://localhost:5000`. Se a API estiver em outra máquina ou porta, altere ali (e no Android use `http://10.0.2.2:5000` para o emulador apontar para o localhost do PC).

---

## Respostas que você recebe (resumo)

| Momento | Endpoint | O que você recebe |
|--------|----------|-------------------|
| Logo após enviar a imagem | `POST /api/video/generate` | `jobId`, `status` (0), `message` |
| Enquanto processa / ao terminar | `GET /api/video/{jobId}/status` | `jobId`, `status` (0–5), `videoUrl` (quando pronto), `errorMessage` (se falhou), flags `scriptGenerated`, `audioGenerated`, `renderSubmitted` |

Valores de **status** (enum numérico): 0 Pending, 1 ScriptGenerated, 2 AudioGenerated, 3 RenderSubmitted, 4 Done, 5 Failed.

---

## Como testar

### Pré-requisito

- API rodando (por exemplo `dotnet run --project Backend/src/Posts.Api`) na porta configurada (ex.: 5000).
- Chaves configuradas (Google AI Studio, ElevenLabs, Shotstack), por exemplo em `appsettings.Development.Local.json`.

### 1. Swagger (navegador)

1. Abra: **http://localhost:5000/swagger**
2. **POST /api/video/generate**
   - Clique em "Try it out".
   - Em **image**, escolha um arquivo (JPEG/PNG/WebP).
   - Opcional: preencha **theme** (ex.: "Páscoa") e **voiceId** (ou deixe em branco para usar o padrão).
   - Execute.
3. Na resposta, copie o **jobId**.
4. **GET /api/video/{jobId}/status**
   - Cole o `jobId` no parâmetro e chame várias vezes até `status` ser 4 (Done) ou 5 (Failed).
   - Se for 4, use o **videoUrl** no navegador para baixar/assistir o vídeo.

### 2. cURL (linha de comando)

Enviar imagem:

```bash
curl -X POST "http://localhost:5000/api/video/generate" \
  -F "image=@C:\caminho\para\sua\imagem.jpg" \
  -F "theme=Páscoa"
```

Resposta esperada (exemplo):

```json
{"jobId":"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx","status":0,"message":"Job submetido. Consulte o status em GET /api/video/{jobId}/status"}
```

Consultar status (troque `SEU_JOB_ID` pelo valor retornado):

```bash
curl "http://localhost:5000/api/video/SEU_JOB_ID/status"
```

Repita o `GET` a cada alguns segundos até `status` ser 4 ou 5.

### 3. App Flutter

1. No `api_config.dart`, confirme que `kBackendBaseUrl` aponta para onde a API está (ex.: `http://localhost:5000`). No **emulador Android**, use `http://10.0.2.2:5000`.
2. Rode o app: `flutter run` na pasta `Frontend`.
3. Toque em **"Enviar imagem"**, escolha uma imagem.
4. Acompanhe o card de status; quando aparecer "Concluído", use **"Abrir vídeo"** para abrir a URL do vídeo.

### 4. Dicas de teste

- **Imagem:** use uma foto de doce, embalagem ou cenário relacionado a "Violeta & Cacau" para o roteiro fazer mais sentido.
- **Tema:** no Swagger ou no cURL, envie `theme=Páscoa` ou `theme=promoção` para variar o roteiro.
- **Erros:** se aparecer **Failed**, veja o `errorMessage` no GET de status (e os logs da API no terminal) para saber se foi Gemini, ElevenLabs ou Shotstack.
- **Shotstack em stage:** no ambiente `stage` a Shotstack pode colocar marca d’água ou limitar duração; para vídeo “limpo” use o ambiente `v1` (produção) e a chave correspondente.

---

## Fluxo em uma frase

Você envia uma **imagem** → a API devolve um **jobId** e processa em background (roteiro → áudio → vídeo) → você consulta **GET /api/video/{jobId}/status** até receber **videoUrl** (status 4) ou **errorMessage** (status 5). O app Flutter faz esse polling sozinho e mostra o status e o link do vídeo quando estiver pronto.
