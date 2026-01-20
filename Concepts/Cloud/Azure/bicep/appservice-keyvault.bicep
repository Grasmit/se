@description('Name prefix for resources')
param namePrefix string = 'demo'
param location string = resourceGroup().location

var webName = '${namePrefix}-web'
var planName = '${namePrefix}-plan'
var kvName = toLower('${namePrefix}kv')

resource plan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: planName
  location: location
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
  }
}

resource web 'Microsoft.Web/sites@2022-03-01' = {
  name: webName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: kvName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: web.identity.principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
  }
}

output webPrincipalId string = web.identity.principalId
output keyVaultUri string = keyVault.properties.vaultUri
