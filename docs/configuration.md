# Конфигурация WebApiApp

## JWT signing key

`Jwt:Key` является секретом и **не должен** находиться в `appsettings.json`, исходном коде, CI-логах или Git.

Приложение требует ключ не менее 32 байт и завершит запуск, если ключ не задан.

### Локальная разработка

Для этого проекта уже настроен .NET User Secrets (`UserSecretsId: webapi-net10-app`). Установить или заменить ключ можно так:

```bash
dotnet user-secrets set --project src/WebApiApp/WebApiApp.csproj "Jwt:Key" "<random-secret>"
```

Проверить наличие настройки, не выводя значение в терминал:

```bash
dotnet user-secrets list --project src/WebApiApp/WebApiApp.csproj
```

Секрет должен быть криптографически случайным. Например, для генерации подходит:

```bash
openssl rand -base64 64
```

### Production

Передавайте секрет через внешний secret store или переменную окружения:

```bash
Jwt__Key=<random-secret>
```

В production следует использовать отдельный ключ, не совпадающий с локальным, и определить процедуру ротации ключей.
