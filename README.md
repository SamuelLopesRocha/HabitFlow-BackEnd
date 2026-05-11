# 📋 API de Hábitos — Documentação

API RESTful desenvolvida em **ASP.NET Core** com banco de dados **MongoDB**. Permite gerenciar hábitos pessoais e compartilhados, amizades, conquistas, notificações e chat entre usuários.

---

## 🔧 Tecnologias

- ASP.NET Core (C#)
- MongoDB
- JWT (autenticação)
- BCrypt (hash de senhas)
- SignalR (mensagens em tempo real)

---

## 🔐 Autenticação

A maioria dos endpoints exige autenticação via **JWT Bearer Token**.

Inclua o header em todas as requisições protegidas:

```
Authorization: Bearer <token>
```

O token é gerado no login e expira em **2 horas**.

---

## 📦 Controllers

### 🔑 Auth — `/api/auth`

Gerencia cadastro, login e verificação de email.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/register` | ❌ | Cadastra novo usuário e envia código de confirmação por email |
| `POST` | `/login` | ❌ | Autentica e retorna JWT + dados do usuário |
| `POST` | `/confirmar-email` | ❌ | Confirma o email com código de 6 dígitos |
| `POST` | `/reenviar-codigo` | ❌ | Reenvia o código de confirmação para o email |

#### `POST /api/auth/register`
```json
{
  "nome": "João Silva",
  "username": "joaosilva",
  "email": "joao@email.com",
  "senha": "minhasenha123"
}
```

#### `POST /api/auth/login`
```json
{
  "email": "joao@email.com",
  "senha": "minhasenha123"
}
```
**Resposta:**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "data": {
    "token": "eyJhbGci...",
    "usuario": {
      "id": "...",
      "nome": "João Silva",
      "username": "joaosilva",
      "email": "joao@email.com"
    }
  }
}
```

#### `POST /api/auth/confirmar-email`
```json
{
  "email": "joao@email.com",
  "codigo": "482910"
}
```

---

### 👤 Usuario — `/api/usuario`

Gerencia perfis de usuários.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/` | ✅ | Lista todos os usuários |
| `GET` | `/me` | ✅ | Retorna o usuário autenticado |
| `GET` | `/{id}` | ✅ | Busca usuário por ID |
| `GET` | `/{username}/perfil` | ❌ | Busca perfil público por username |
| `POST` | `/` | ❌ | Cria novo usuário |
| `PUT` | `/{id}` | ✅ | Atualiza dados do usuário |
| `DELETE` | `/{id}` | ✅ | Remove usuário |

#### `PUT /api/usuario/{id}` — campos atualizáveis
```json
{
  "nome": "Novo Nome",
  "email": "novo@email.com",
  "fotoPerfilUrl": "https://...",
  "dataNascimento": "1998-05-20",
  "temaPreferido": "dark"
}
```

---

### 🤝 Amizade — `/api/amizade`

Gerencia solicitações e vínculos de amizade entre usuários. Ao aceitar uma amizade, um chat privado é criado automaticamente.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Envia solicitação de amizade por username |
| `POST` | `/responder` | ✅ | Aceita ou recusa uma solicitação |
| `POST` | `/bloquear/{amizadeId}` | ✅ | Bloqueia um usuário |
| `GET` | `/amigos` | ✅ | Lista amigos confirmados |
| `GET` | `/pendentes` | ✅ | Lista solicitações recebidas pendentes |
| `GET` | `/enviados` | ✅ | Lista solicitações enviadas pendentes |

#### `POST /api/amizade`
```json
{ "username": "joaosilva" }
```

#### `POST /api/amizade/responder`
```json
{
  "amizadeId": "abc123",
  "aceitar": true
}
```

---

### 🏃 Hábitos — `/api/habitos`

CRUD completo de hábitos pessoais do usuário autenticado.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Cria novo hábito |
| `GET` | `/` | ✅ | Lista hábitos do usuário |
| `GET` | `/{id}` | ✅ | Busca hábito por ID |
| `PUT` | `/{id}` | ✅ | Atualiza hábito |
| `DELETE` | `/{id}` | ✅ | Remove hábito e notifica |

#### `POST /api/habitos` — body
```json
{
  "nome": "Beber água",
  "descricao": "2L por dia",
  "cor": "#00bcd4",
  "icone": "💧",
  "tipo": "Quantidade",
  "frequencia": "Diaria",
  "meta": 2000,
  "unidade": "ml",
  "horaPreferida": "08:00",
  "notificacaoAtiva": true,
  "prioridade": "Alta",
  "dataInicio": "2024-01-01"
}
```

---

### 📅 Registros de Hábito — `/api/habitorecorde`

Registra a execução diária de hábitos. Calcula streak automaticamente a cada registro.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Registra execução do hábito (1 por dia) |
| `GET` | `/{habitId}` | ✅ | Lista todos os registros de um hábito |
| `GET` | `/{habitId}/calendario` | ✅ | Retorna calendário estilo GitHub (30 dias) |
| `GET` | `/registro/{id}` | ✅ | Busca registro por ID |
| `PUT` | `/{id}` | ✅ | Atualiza registro existente |
| `DELETE` | `/{id}` | ✅ | Remove registro |

#### `POST /api/habitorecorde`
```json
{
  "habitId": "guid-do-habito",
  "quantidade": 2000,
  "concluido": true,
  "observacao": "Consegui beber tudo!"
}
```

#### `GET /api/habitorecorde/{habitId}/calendario`
Retorna array de 30 dias no estilo heatmap (GitHub contributions):
```json
[
  { "data": "2024-01-01", "concluido": true },
  { "data": "2024-01-02", "concluido": false },
  ...
]
```

> **Streak**: ao criar ou atualizar um registro, `streakAtual` e `melhorStreak` do hábito são recalculados automaticamente.

---

### 🤲 Hábitos Compartilhados — `/api/habitos-compartilhados`

Permite criar hábitos colaborativos com múltiplos participantes e acompanhar o progresso em grupo.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Cria hábito compartilhado (criador vira participante) |
| `POST` | `/{id}/convidar` | ✅ | Convida usuário por username (somente criador) |
| `POST` | `/convite/{id}/aceitar` | ✅ | Aceita convite de participação |
| `POST` | `/convite/{id}/recusar` | ✅ | Recusa convite |
| `GET` | `/convites` | ✅ | Lista convites pendentes recebidos |
| `GET` | `/{id}/progresso` | ✅ | Ranking de progresso dos participantes |

#### `POST /api/habitos-compartilhados`
```json
{
  "habitId": "guid-do-habito",
  "nome": "Desafio 30 dias",
  "metaCompartilhada": 30,
  "tipo": "Contagem",
  "periodo": "Mensal"
}
```

#### `GET /api/habitos-compartilhados/{id}/progresso`
```json
[
  { "userId": "abc", "total": 28 },
  { "userId": "xyz", "total": 21 }
]
```

---

### 🏆 Conquistas — `/api/conquista`

Catálogo de conquistas e histórico por usuário. Conquistas são verificadas automaticamente ao concluir hábitos.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Cria conquista no catálogo |
| `GET` | `/` | ✅ | Lista todas as conquistas disponíveis |
| `GET` | `/minhas` | ✅ | Lista conquistas do usuário autenticado |
| `GET` | `/{id}` | ✅ | Busca conquista por ID |
| `PUT` | `/{id}` | ✅ | Atualiza conquista |
| `DELETE` | `/{id}` | ✅ | Remove conquista |

#### `POST /api/conquista`
```json
{
  "nome": "Semana Perfeita",
  "descricao": "Complete hábitos por 7 dias seguidos",
  "icone": "🔥",
  "tipo": "Streak",
  "valorMeta": 7
}
```

---

### 🔔 Notificações — `/api/notificacao`

Sistema de notificações internas (amizade, hábitos, sistema).

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/` | ✅ | Lista todas as notificações |
| `GET` | `/nao-lidas` | ✅ | Lista apenas não lidas |
| `PATCH` | `/{id}/ler` | ✅ | Marca notificação como lida |
| `PATCH` | `/ler-todas` | ✅ | Marca todas como lidas |
| `DELETE` | `/{id}` | ✅ | Remove notificação |
| `POST` | `/teste` | ✅ | Cria notificação de teste |

---

### 💬 Chat — `/api/chat`

Gerencia conversas privadas e em grupo. Chats privados são criados automaticamente ao aceitar amizades.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/` | ✅ | Cria novo chat (privado ou grupo) |
| `GET` | `/` | ✅ | Lista chats do usuário |
| `POST` | `/{chatId}/sair` | ✅ | Sair de um chat |
| `DELETE` | `/{chatId}` | ✅ | Deleta chat |

#### `POST /api/chat`
```json
{
  "tipo": "Grupo",
  "usernames": ["joaosilva", "mariasouza"]
}
```

---

### ✉️ Mensagens — `/api/mensagem`

Envio de mensagens dentro de um chat.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/{chatId}` | ✅ | Envia mensagem em um chat |

#### `POST /api/mensagem/{chatId}`
```json
"Olá, tudo bem?"
```

---

## 📊 Padrão de Resposta

A maioria dos endpoints usa o envelope `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Descrição da operação",
  "data": { }
}
```

Endpoints de erro retornam objetos com campo `mensagem` ou strings diretas dependendo do controller.

---

## ⚙️ Configuração

Variáveis necessárias em `appsettings.json`:

```json
{
  "AllowedHosts": "*",
  "MongoDB": {
    "ConnectionString": "sua chave aqui",
    "DatabaseName": "nome da database"
  }
}
```