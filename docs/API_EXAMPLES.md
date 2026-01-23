# Exemplos de Requisições para Analisador ATS API

## 1. Análise Genérica (sem vaga)

### cURL

```bash
curl -X POST "http://localhost:5284/api/ats/analyze" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/caminho/para/curriculo.pdf"
```

### PowerShell

```powershell
$uri = "http://localhost:5284/api/ats/analyze"
$filePath = "C:\caminho\para\curriculo.pdf"

$form = @{
    file = Get-Item -Path $filePath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form
```

---

## 2. Análise com Vaga

### cURL

```bash
curl -X POST "http://localhost:5284/api/ats/analyze-job" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/caminho/para/curriculo.pdf" \
  -F 'jobDescription={
    "title": "Desenvolvedor .NET Sênior",
    "requiredSkills": ["c#", ".net core", "sql server", "azure", "rest api"],
    "desiredSkills": ["docker", "kubernetes", "kafka", "redis"],
    "minimumExperience": 5,
    "seniority": "Senior",
    "description": "Buscamos desenvolvedor .NET com experiência em cloud"
  };type=application/json'
```

### PowerShell

```powershell
$uri = "http://localhost:5284/api/ats/analyze-job"
$filePath = "C:\caminho\para\curriculo.pdf"

$jobDescription = @{
    title = "Desenvolvedor .NET Sênior"
    requiredSkills = @("c#", ".net core", "sql server", "azure", "rest api")
    desiredSkills = @("docker", "kubernetes", "kafka")
    minimumExperience = 5
    seniority = "Senior"
    description = "Buscamos desenvolvedor .NET com experiência em cloud"
} | ConvertTo-Json

$form = @{
    file = Get-Item -Path $filePath
    jobDescription = $jobDescription
}

$response = Invoke-RestMethod -Uri $uri -Method Post -Form $form
$response | ConvertTo-Json -Depth 10
```

---

## 3. Exemplo com C# HttpClient

```csharp
using System.Net.Http.Headers;

var client = new HttpClient();
var apiUrl = "http://localhost:5284/api/ats/analyze-job";

// Criar o multipart form
var form = new MultipartFormDataContent();

// Adicionar arquivo
var fileContent = new ByteArrayContent(File.ReadAllBytes(@"C:\caminho\para\curriculo.pdf"));
fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
form.Add(fileContent, "file", "curriculo.pdf");

// Adicionar job description
var jobDescription = new
{
    title = "Desenvolvedor .NET Sênior",
    requiredSkills = new[] { "c#", ".net core", "sql server", "azure" },
    desiredSkills = new[] { "docker", "kubernetes" },
    minimumExperience = 5,
    seniority = "Senior"
};

var jobJson = JsonSerializer.Serialize(jobDescription);
var jobContent = new StringContent(jobJson, Encoding.UTF8, "application/json");
form.Add(jobContent, "jobDescription");

// Enviar requisição
var response = await client.PostAsync(apiUrl, form);
var result = await response.Content.ReadAsStringAsync();

Console.WriteLine(result);
```

---

## 4. Exemplo com JavaScript/Fetch

```javascript
async function analyzeResume() {
  const fileInput = document.getElementById('fileInput');
  const file = fileInput.files[0];

  const formData = new FormData();
  formData.append('file', file);

  const jobDescription = {
    title: "Desenvolvedor .NET Sênior",
    requiredSkills: ["c#", ".net core", "sql server", "azure"],
    desiredSkills: ["docker", "kubernetes"],
    minimumExperience: 5,
    seniority: "Senior"
  };

  formData.append('jobDescription', JSON.stringify(jobDescription));

  const response = await fetch('http://localhost:5284/api/ats/analyze-job', {
    method: 'POST',
    body: formData
  });

  const result = await response.json();
  console.log(result);
}
```

---

## 5. Exemplo com Python (requests)

```python
import requests
import json

url = "http://localhost:5284/api/ats/analyze-job"

files = {
    'file': open('curriculo.pdf', 'rb')
}

job_description = {
    "title": "Desenvolvedor .NET Sênior",
    "requiredSkills": ["c#", ".net core", "sql server", "azure"],
    "desiredSkills": ["docker", "kubernetes"],
    "minimumExperience": 5,
    "seniority": "Senior"
}

data = {
    'jobDescription': json.dumps(job_description)
}

response = requests.post(url, files=files, data=data)
print(response.json())
```

---

## Exemplo de Resposta

### Análise Genérica

```json
{
  "overall_score": 82,
  "ats_compatibility_score": 78,
  "extracted_data": {
    "name": "João Silva",
    "email": "joao.silva@email.com",
    "phone": "(11) 98765-4321",
    "estimated_seniority": "Senior",
    "skills": [
      "c#",
      ".net core",
      "sql server",
      "azure",
      "docker",
      "react"
    ]
  },
  "strengths": [
    "Ampla variedade de skills técnicas identificadas",
    "Senioridade claramente identificada como Senior",
    "Informações de contato completas"
  ],
  "weaknesses": [],
  "suggestions": [
    {
      "title": "Adicionar métricas e resultados",
      "description": "Inclua números concretos sobre impacto dos projetos"
    }
  ],
  "keyword_analysis": {
    "missing": [],
    "present": [
      "c#",
      ".net core",
      "sql server",
      "azure",
      "docker",
      "react"
    ],
    "recommended": [
      "kubernetes",
      "microservices",
      "ci/cd"
    ]
  },
  "formatting_feedback": "Excelente! Currículo possui formatação ideal para ATS."
}
```

### Análise com Vaga

```json
{
  "overall_score": 89,
  "ats_compatibility_score": 78,
  "job_match_score": 95,
  "extracted_data": {
    "name": "João Silva",
    "email": "joao.silva@email.com",
    "phone": "(11) 98765-4321",
    "estimated_seniority": "Senior",
    "skills": [
      "c#",
      ".net core",
      "sql server",
      "azure",
      "docker",
      "rest api"
    ]
  },
  "job_requirements_match": {
    "required_skills_met": 5,
    "required_skills_total": 5,
    "desired_skills_met": 1,
    "desired_skills_total": 3,
    "experience_match": true
  },
  "strengths": [
    "Possui todas as skills obrigatórias da vaga",
    "Senioridade compatível com o requisitado",
    "Possui 1 das 3 skills desejáveis"
  ],
  "weaknesses": [],
  "suggestions": [
    {
      "title": "Destacar experiência com tecnologias da vaga",
      "description": "Se possui experiência, adicione projetos específicos onde utilizou: kubernetes, kafka"
    },
    {
      "title": "Customizar currículo para a vaga",
      "description": "Destaque experiências relacionadas a 'Desenvolvedor .NET Sênior'"
    }
  ],
  "keyword_analysis": {
    "missing": [
      "kubernetes",
      "kafka"
    ],
    "present": [
      "c#",
      ".net core",
      "sql server",
      "azure",
      "docker",
      "rest api"
    ],
    "recommended": [
      "microservices"
    ]
  },
  "formatting_feedback": "Excelente! Currículo possui formatação ideal para ATS."
}
```

---

## Testes Rápidos com Swagger

A maneira mais fácil de testar é através do Swagger UI:

1. Acesse: http://localhost:5284
2. Expanda o endpoint desejado
3. Clique em "Try it out"
4. Faça upload do arquivo
5. (Para analyze-job) Cole o JSON da vaga
6. Clique em "Execute"

O Swagger mostrará a resposta formatada automaticamente!
