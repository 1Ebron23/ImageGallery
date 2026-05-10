targetScope = 'resourceGroup'

@description('Sovelluksen nimi (maailmanlaajuisesti uniikki)')
param appName string

@description('Storage account name')
param storageAccountName string

@description('Azure-sijainti')
param location string = resourceGroup().location

@description('ModerationService API-avain — ei tallenneta tiedostoon!')
@secure()
param moderationApiKey string

// Johda resurssien nimet parametrista
var keyVaultName = 'kv-${appName}'
var appServicePlanName = 'plan-${appName}'

// ── Storage Account ────────────────────────────────────────────────────────
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/photos'
  properties: {
    publicAccess: 'None'
  }
}

// ── App Service Plan ───────────────────────────────────────────────────────
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// ── App Service (Managed Identity aktivoitu suoraan) ─────────────────────
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        { name: 'Storage__Provider',     value: 'azure' }
        { name: 'Storage__AccountName',  value: storageAccountName }
        { name: 'Storage__ContainerName',value: 'photos' }
        { name: 'KeyVault__VaultUrl',    value: 'https://${keyVaultName}.vault.azure.net/' }
      ]
    }
  }
}

// ── Key Vault ──────────────────────────────────────────────────────────────
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
  }
}

resource moderationSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'ModerationService--ApiKey'
  properties: {
    value: moderationApiKey
  }
}

// ── RBAC: App Service → Blob Storage ──────────────────────────────────────
resource storageBlobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, webApp.id, 'StorageBlobDataContributor')
  scope: storageAccount
  properties: {
    // Storage Blob Data Contributor -roolin sisäänrakennettu GUID
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── RBAC: App Service → Key Vault ─────────────────────────────────────────
resource keyVaultSecretsRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.id, 'KeyVaultSecretsUser')
  scope: keyVault
  properties: {
    // Key Vault Secrets User -roolin sisäänrakennettu GUID
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── Tulosteet ──────────────────────────────────────────────────────────────
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output storageAccountName string = storageAccountName
output keyVaultName string = keyVaultName