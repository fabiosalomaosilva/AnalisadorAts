namespace AnalisadorAts.Infrastructure.Extraction;

public class SkillsDictionary
{
    private readonly Dictionary<string, List<string>> _skillCategories;
    private readonly HashSet<string> _allSkills;

    public SkillsDictionary()
    {
        _skillCategories = new Dictionary<string, List<string>>
        {
            ["backend"] = new List<string>
            {
                "c#", "csharp", ".net", "dotnet", "asp.net", "asp.net core", "net core",
                "java", "spring", "spring boot", "python", "django", "flask", "fastapi",
                "node.js", "nodejs", "express", "nestjs", "php", "laravel", "symfony",
                "ruby", "rails", "go", "golang", "rust"
            },
            ["frontend"] = new List<string>
            {
                "react", "reactjs", "react.js", "angular", "angularjs", "vue", "vue.js", "vuejs",
                "javascript", "typescript", "html", "css", "sass", "scss", "tailwind",
                "next.js", "nextjs", "nuxt", "svelte", "bootstrap", "material-ui", "mui"
            },
            ["database"] = new List<string>
            {
                "sql", "sql server", "mssql", "mysql", "postgresql", "postgres", "oracle",
                "mongodb", "cosmosdb", "dynamodb", "redis", "cassandra", "elasticsearch",
                "sqlite", "mariadb", "firebase"
            },
            ["cloud"] = new List<string>
            {
                "azure", "aws", "gcp", "google cloud", "kubernetes", "k8s", "docker",
                "terraform", "ansible", "jenkins", "github actions", "gitlab ci",
                "azure devops", "cloud computing"
            },
            ["devops"] = new List<string>
            {
                "ci/cd", "devops", "git", "github", "gitlab", "bitbucket",
                "docker", "kubernetes", "jenkins", "terraform", "ansible",
                "prometheus", "grafana", "elk", "nginx", "apache"
            },
            ["architecture"] = new List<string>
            {
                "microservices", "microservicos", "api rest", "rest api", "restful",
                "graphql", "grpc", "soap", "event-driven", "message broker",
                "kafka", "rabbitmq", "azure service bus", "clean architecture",
                "ddd", "domain-driven design", "cqrs", "solid", "design patterns"
            },
            ["testing"] = new List<string>
            {
                "tdd", "bdd", "unit test", "integration test", "xunit", "nunit",
                "jest", "mocha", "pytest", "junit", "selenium", "cypress"
            },
            ["mobile"] = new List<string>
            {
                "react native", "flutter", "xamarin", "ionic", "swift", "kotlin",
                "android", "ios", "mobile"
            },
            ["data"] = new List<string>
            {
                "data science", "machine learning", "ml", "ai", "deep learning",
                "pandas", "numpy", "scikit-learn", "tensorflow", "pytorch",
                "power bi", "tableau", "data analytics", "etl"
            }
        };

        _allSkills = _skillCategories.Values
            .SelectMany(skills => skills)
            .ToHashSet();
    }

    public List<string> GetAllSkills() => _allSkills.ToList();

    public List<string> GetSkillsByCategory(string category)
    {
        return _skillCategories.TryGetValue(category.ToLower(), out var skills)
            ? skills
            : new List<string>();
    }

    public List<string> GetRecommendedSkills(List<string> currentSkills)
    {
        var recommended = new List<string>();

        // Se tem C#/.NET, recomendar cloud e patterns
        if (currentSkills.Any(s => s.Contains("c#") || s.Contains(".net") || s.Contains("csharp")))
        {
            recommended.AddRange(new[] { "azure", "docker", "kubernetes", "microservices", "clean architecture" });
        }

        // Se tem React, recomendar ecosystem
        if (currentSkills.Any(s => s.Contains("react")))
        {
            recommended.AddRange(new[] { "next.js", "typescript", "graphql", "tailwind" });
        }

        // Se tem backend, recomendar database e cloud
        var hasBackend = currentSkills.Any(s =>
            _skillCategories["backend"].Contains(s));

        if (hasBackend)
        {
            recommended.AddRange(new[] { "docker", "kubernetes", "ci/cd", "microservices" });
        }

        return recommended.Distinct()
            .Where(r => !currentSkills.Contains(r))
            .Take(5)
            .ToList();
    }
}
