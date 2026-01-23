# 📄 Analisador ATS - API

Sistema de análise de currículos compatível com ATS (Applicant Tracking System) desenvolvido em ASP.NET Core (.NET 9.0).

## 🎯 Funcionalidades

- ✅ **Parsing de currículos** - Suporte para PDF e DOCX
- ✅ **Extração automática de dados** - Nome, e-mail, telefone, skills
- ✅ **Análise genérica** - Score ATS, pontos fortes e fracos
- ✅ **Análise com vaga** - Matching entre currículo e requisitos da vaga
- ✅ **Feedback de formatação** - Sugestões de melhoria para compatibilidade ATS
- ✅ **Dicionário de skills** - 100+ tecnologias categorizadas

## 🏗️ Arquitetura

```
AnalisadorAts/
├── src/
│   ├── AnalisadorAts.Api/              # Web API + Controllers
│   ├── AnalisadorAts.Core/             # Modelos e Interfaces
│   └── AnalisadorAts.Infrastructure/   # Parsers, Extração, Análise
└── docs/
    └── SPEC.MD                         # Especificação técnica
```

### Tecnologias Utilizadas

- **ASP.NET Core 9.0** - Framework web
- **Swashbuckle** - Documentação OpenAPI/Swagger
- **UglyToad.PdfPig** - Parsing de PDF
- **DocumentFormat.OpenXml** - Parsing de DOCX

## 🚀 Como Executar

### Pré-requisitos

- .NET 9.0 SDK
- Visual Studio 2022 ou VS Code

### Executar

```bash
cd d:\Projetos\AnalisadorAts
dotnet build
dotnet run --project src/AnalisadorAts.Api/AnalisadorAts.Api.csproj
```

A API estará disponível em: **http://localhost:5284**

Swagger UI: **http://localhost:5284** (aberto por padrão)

## 📡 Endpoints

### 1. Análise Genérica

**`POST /api/ats/analyze`**

Analisa um currículo sem considerar uma vaga específica.

**Request:**
- `file` (form-data): Arquivo PDF ou DOCX

**Response:**
```json
{
  "overall_score": 82,
  "ats_compatibility_score": 78,
  "extracted_data": {
    "name": "João Silva",
    "email": "joao@email.com",
    "phone": "(11) 98765-4321",
    "estimated_seniority": "Senior",
    "skills": ["c#", ".net", "sql server", "azure"]
  },
  "strengths": [
    "Ampla variedade de skills técnicas identificadas",
    "Currículo bem estruturado e ATS-friendly"
  ],
  "weaknesses": [],
  "suggestions": [
    {
      "title": "Adicionar métricas e resultados",
      "description": "Inclua números concretos sobre impacto..."
    }
  ],
  "keyword_analysis": {
    "missing": [],
    "present": ["c#", ".net", "sql server", "azure"],
    "recommended": ["docker", "kubernetes", "microservices"]
  },
  "formatting_feedback": "Excelente! Currículo possui formatação ideal..."
}
```

### 2. Análise com Vaga

**`POST /api/ats/analyze-job`**

Analisa compatibilidade entre currículo e requisitos da vaga.

**Request:**
- `file` (form-data): Arquivo PDF ou DOCX
- `jobDescription` (form-data, JSON):
```json
{
  "title": "Desenvolvedor .NET Sênior",
  "requiredSkills": ["c#", ".net core", "sql server", "azure"],
  "desiredSkills": ["docker", "kubernetes"],
  "minimumExperience": 5,
  "seniority": "Senior"
}
```

**Response:**
```json
{
  "overall_score": 85,
  "ats_compatibility_score": 78,
  "job_match_score": 91,
  "extracted_data": { "...": "..." },
  "job_requirements_match": {
    "required_skills_met": 4,
    "required_skills_total": 4,
    "desired_skills_met": 1,
    "desired_skills_total": 2,
    "experience_match": true
  },
  "strengths": [
    "Possui todas as skills obrigatórias da vaga",
    "Senioridade compatível com o requisitado"
  ],
  "weaknesses": [
    "Falta experiência documentada em Kubernetes"
  ],
  "suggestions": [ "..."],
  "keyword_analysis": {
    "missing": ["kubernetes"],
    "present": ["c#", ".net core", "sql server", "azure", "docker"],
    "recommended": ["microservices"]
  },
  "formatting_feedback": "..."
}
```

## 🎨 Testando no Swagger

1. Acesse http://localhost:5284
2. Selecione o endpoint desejado
3. Clique em "Try it out"
4. Faça upload de um arquivo PDF/DOCX
5. (Para analyze-job) Adicione o JSON da vaga
6. Execute!

## 📊 Sistema de Pontuação

### Overall Score (0-100)
- **30%** - Compatibilidade ATS
- **40%** - Quantidade e relevância de skills
- **15%** - Dados básicos (nome, email, telefone)
- **15%** - Senioridade identificada

### ATS Compatibility Score (0-100)
- Penalidades por:
  - Tabelas complexas (-15)
  - Múltiplas colunas (-10)
  - Currículo muito curto (-20)

### Job Match Score (0-100)
- **60%** - Skills obrigatórias atendidas
- **30%** - Skills desejáveis atendidas
- **10%** - Senioridade/experiência compatível

## 🧠 Dicionário de Skills

O sistema reconhece 100+ tecnologias organizadas em categorias:

- **Backend**: C#, .NET, Java, Python, Node.js...
- **Frontend**: React, Angular, Vue, TypeScript...
- **Database**: SQL Server, PostgreSQL, MongoDB...
- **Cloud**: Azure, AWS, GCP, Docker, Kubernetes...
- **DevOps**: CI/CD, Git, Jenkins, Terraform...
- **Architecture**: Microservices, REST API, GraphQL...

## ⚙️ Configurações

### Limites
- Tamanho máximo do arquivo: **5MB**
- Formatos aceitos: **PDF, DOCX**

### CORS
Por padrão, a API aceita requisições de qualquer origem no modo Development.

## 🔍 Extração de Dados

### Regex Patterns
- **Email**: `[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}`
- **Telefone**: `\(?\d{2}\)?\s?\d{4,5}-?\d{4}`

### Normalização de Texto
1. Conversão para lowercase
2. Remoção de acentos
3. Remoção de caracteres especiais
4. Normalização de espaços

## 🐛 Tratamento de Erros

A API retorna erros estruturados:

```json
{
  "success": false,
  "error": {
    "code": "INVALID_FILE",
    "message": "Formato não suportado. Apenas PDF e DOCX são permitidos",
    "details": "..."
  }
}
```

### Códigos de Erro
- `INVALID_FILE` - Arquivo inválido ou muito grande
- `UNSUPPORTED_FORMAT` - Formato não suportado
- `INVALID_JOB_DESCRIPTION` - Descrição da vaga inválida
- `PROCESSING_ERROR` - Erro no processamento

## 📝 Melhorias Futuras

- [ ] Suporte a múltiplos idiomas
- [ ] Cache de skills dictionary
- [ ] Análise de experiência temporal
- [ ] Extração de formação acadêmica
- [ ] API de gerenciamento de skills customizadas
- [ ] Exportação de relatórios em PDF

## 📄 Licença

Projeto desenvolvido para demonstração de sistema ATS sem IA generativa.
