## 🎯 O que um ATS “sem IA generativa” consegue fazer muito bem

Um analisador ATS tradicional consegue:

✅ Parsear currículos (PDF, DOCX)
✅ Extrair dados estruturados (nome, e-mail, skills, experiências)
✅ Calcular score de compatibilidade com a vaga
✅ Identificar palavras-chave ausentes
✅ Avaliar formatação ATS-friendly
✅ Classificar currículos por relevância
✅ Detectar senioridade aproximada

> **Isso é exatamente como ATS comerciais funcionavam antes de LLMs**.

---

## 🧱 Arquitetura sugerida (.NET 10)

```text
[Upload CV]
   ↓
[Parser Documento]
   ↓
[Normalização de Texto]
   ↓
[Extração de Dados]
   ↓
[Motor de Regras ATS]
   ↓
[Score + Relatório]
```

---

## 📄 1. Parsing de currículo (PDF / DOCX)

### Bibliotecas

* **PDF**: `UglyToad.PdfPig`, `iText7`
* **DOCX**: `OpenXML SDK`
* **TXT**: direto

```csharp
string texto = PdfParser.ExtractText(file);
```

⚠️ Importante:

* Ignorar tabelas complexas
* Evitar múltiplas colunas
* Detectar headers e footers

---

## 🧹 2. Normalização de texto (crítico)

```csharp
texto = texto.ToLowerInvariant();
texto = RemoveAcentos(texto);
texto = Regex.Replace(texto, @"[^a-z0-9\s]", " ");
texto = Regex.Replace(texto, @"\s+", " ");
```

✔️ ATS reais fazem isso antes de qualquer análise

---

## 🧠 3. Extração de dados (sem IA)

### 🔍 Regex + Dicionários

**Email**

```csharp
Regex emailRegex = new(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}");
```

**Telefone**

```csharp
Regex phoneRegex = new(@"\(?\d{2}\)?\s?\d{4,5}-?\d{4}");
```

**Skills**

```json
["c#", ".net", "asp.net", "sql", "azure", "docker", "kafka"]
```

```csharp
skillsEncontradas = skills.Where(s => texto.Contains(s));
```

---

## 🏗️ 4. Motor de Regras ATS (core do sistema)

### Exemplo de pesos

```json
{
  "skills": 50,
  "experiencia": 30,
  "formacao": 10,
  "formato": 10
}
```

```csharp
score += skillsMatch * 0.5;
score += experienciaMatch * 0.3;
```

---

## 📊 5. Matching vaga x currículo (sem ML)

### Estratégias clássicas

| Técnica            | Descrição            |
| ------------------ | -------------------- |
| Keyword Matching   | % de palavras-chave  |
| TF-IDF             | Importância relativa |
| Cosine Similarity  | Similaridade textual |
| Rule-based scoring | Pontuação fixa       |

### TF-IDF + Cosine Similarity

👉 **Totalmente possível sem ML.NET**

---

## 🤖 6. Usando ML.NET (opcional)

Você **pode usar ML.NET sem IA generativa**, apenas ML clássico.

### Casos ideais para ML.NET:

✅ Classificar currículo (Júnior / Pleno / Sênior)
✅ Prever fit com vaga (0–100)
✅ Detectar área principal (Backend, Frontend, Data)

### Algoritmos comuns:

* Logistic Regression
* FastTree
* SDCA
* Naive Bayes

```csharp
var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", "Texto")
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
```

✔️ Totalmente on-premise
✔️ Sem dependência externa
✔️ Sem LLM

---

## 📋 7. Análise de formatação ATS

Regras simples:

❌ Muitas colunas
❌ Tabelas
❌ Ícones / imagens
❌ Fontes não padrão

```csharp
if (DetectaTabela(pdf))
    penalidade += 10;
```

---

## 📈 8. Relatório final ATS

Exemplo:

```json
{
  "scoreFinal": 82,
  "skillsFaltantes": ["kafka", "redis"],
  "pontosFortes": ["c#", ".net", "microservices"],
  "nivelEstimado": "Senior",
  "atsFriendly": true
}
```

---

## ⚠️ Limitações SEM IA generativa

| Limitação                 | Impacto                          |
| ------------------------- | -------------------------------- |
| Sem semântica profunda    | “.NET” ≠ “DotNet” sem dicionário |
| Difícil entender contexto | “liderança” ≠ “gerenciou equipe” |
| Multilíngue limitado      | precisa regras por idioma        |
| Manutenção de regras      | exige curadoria                  |

> ATS tradicionais **aceitam essas limitações**.

---

## 🧠 Comparação direta

| Abordagem    | Precisão | Custo  | Controle | Explicabilidade |
| ------------ | -------- | ------ | -------- | --------------- |
| Regras + NLP | ⭐⭐⭐⭐     | 💲     | ⭐⭐⭐⭐⭐    | ⭐⭐⭐⭐⭐           |
| ML.NET       | ⭐⭐⭐⭐     | 💲💲   | ⭐⭐⭐⭐     | ⭐⭐⭐⭐            |
| LLM (GPT)    | ⭐⭐⭐⭐⭐    | 💲💲💲 | ⭐⭐       | ⭐⭐              |

---

## ✅ Conclusão

✔️ **Sim, é totalmente viável** criar um ATS completo **sem IA generativa** em **.NET 10**
✔️ Inclusive **mais previsível e auditável**
✔️ ML.NET entra como **opcional**, não obrigatório
✔️ Ideal para:

* Compliance
* RH corporativo
* Baixo custo
* Alta explicabilidade
