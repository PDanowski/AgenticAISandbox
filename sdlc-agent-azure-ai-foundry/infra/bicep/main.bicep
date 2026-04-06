targetScope = 'resourceGroup'

@description('Primary Azure region.')
param location string

@description('Environment name (dev/test/prod).')
param environmentName string

@description('Common name prefix used for resources.')
param namePrefix string

@description('Globally unique Azure Container Registry name (alphanumeric only).')
param acrName string

@description('Image for orchestrator service.')
param imageOrchestrator string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Image for architect service.')
param imageArchitect string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Image for developer service.')
param imageDeveloper string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Image for devops service.')
param imageDevOps string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Image for QA service.')
param imageQa string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('Optional Azure AI Foundry project endpoint.')
param foundryProjectEndpoint string = ''

@description('Optional Azure AI Foundry model deployment name.')
param foundryModelDeployment string = ''

@description('Optional managed identity client ID for Foundry auth.')
param foundryManagedIdentityClientId string = ''

param tags object = {}

var workspaceName = '${namePrefix}-law'
var containerAppsEnvironmentName = '${namePrefix}-cae'
var userAssignedIdentityName = '${namePrefix}-ca-mi'
var orchestratorAppName = '${namePrefix}-orchestrator'
var architectAppName = '${namePrefix}-architect'
var developerAppName = '${namePrefix}-developer'
var devOpsAppName = '${namePrefix}-devops'
var qaAppName = '${namePrefix}-qa'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: union(tags, {
    environment: environmentName
  })
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  tags: union(tags, {
    environment: environmentName
  })
  properties: {
    adminUserEnabled: false
  }
}

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentityName
  location: location
  tags: union(tags, {
    environment: environmentName
  })
}

var workspaceKeys = listKeys(logAnalytics.id, '2022-10-01')

resource managedEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppsEnvironmentName
  location: location
  tags: union(tags, {
    environment: environmentName
  })
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: workspaceKeys.primarySharedKey
      }
    }
  }
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, userAssignedIdentity.id, 'acrpull')
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource architectApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: architectAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    environment: environmentName
    role: 'architect'
  })
  properties: {
    managedEnvironmentId: managedEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'architect-api'
          image: imageArchitect
          resources: {
            cpu: 0.5
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

resource developerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: developerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    environment: environmentName
    role: 'developer'
  })
  properties: {
    managedEnvironmentId: managedEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'developer-api'
          image: imageDeveloper
          resources: {
            cpu: 0.5
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

resource devOpsApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: devOpsAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    environment: environmentName
    role: 'devops'
  })
  properties: {
    managedEnvironmentId: managedEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'devops-api'
          image: imageDevOps
          resources: {
            cpu: 0.5
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

resource qaApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: qaAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    environment: environmentName
    role: 'qa'
  })
  properties: {
    managedEnvironmentId: managedEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'qa-api'
          image: imageQa
          resources: {
            cpu: 0.5
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

resource orchestratorApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: orchestratorAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    environment: environmentName
    role: 'orchestrator'
  })
  properties: {
    managedEnvironmentId: managedEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'orchestrator-api'
          image: imageOrchestrator
          resources: {
            cpu: 1
            memory: '2Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'AgentServices__Architect'
              value: 'https://${architectApp.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'AgentServices__Developer'
              value: 'https://${developerApp.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'AgentServices__DevOps'
              value: 'https://${devOpsApp.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'AgentServices__Qa'
              value: 'https://${qaApp.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'AzureAiFoundry__ProjectEndpoint'
              value: foundryProjectEndpoint
            }
            {
              name: 'AzureAiFoundry__DefaultModelDeployment'
              value: foundryModelDeployment
            }
            {
              name: 'AzureAiFoundry__ManagedIdentityClientId'
              value: foundryManagedIdentityClientId
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  dependsOn: [
    architectApp
    developerApp
    devOpsApp
    qaApp
  ]
}

output acrLoginServer string = acr.properties.loginServer
output managedEnvironmentId string = managedEnvironment.id
output orchestratorUrl string = 'https://${orchestratorApp.properties.configuration.ingress.fqdn}'
