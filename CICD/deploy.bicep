targetScope='subscription'

param resourceGroupLocation string = 'centralus'

@allowed([
  'dev'
  'qa'
  'prod'
])
param environmentName string


var appName = 'BacktestApi'

resource newRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: '${appName}-${environmentName}'
  location: resourceGroupLocation
}

module functionApp 'azure-function.bicep' = {
  name: 'storageModule'
  scope: newRG
  params:{
    deploymentEnvironment: environmentName
    appName: appName
  }
}
