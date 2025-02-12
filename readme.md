

# codigo para listar los contenedores de azure en c#

instalar dependencias

![alt text](image-2.png)

```
using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;


class Program
{
    static async Task Main(string[] args)
    {
        string endpoint = "https://st02022025-secondary.blob.core.windows.net/";
        var credential = new DefaultAzureCredential();
        BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(endpoint), credential);
        var endpointUri = blobServiceClient.Uri;
        Console.WriteLine("Connnected to "+ endpointUri);

        await ListContainersInAccount(blobServiceClient);

    }

    static async Task ListContainersInAccount(BlobServiceClient blobServiceClient) 
    {
        Console.WriteLine("listing container info ....");
        await foreach (var container in blobServiceClient.GetBlobContainersAsync()) {
            Console.WriteLine($"Container: {container.Name}");
        }
    }
}
```



modificar container properties .net

![alt text](image.png)

modificar container properties powershell

![alt text](image-1.png)


```
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "connection str";
        string containerName = "demo-1";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var metadata = new Dictionary<string, string>
        {
            { "owner", "admin" },
            { "environment", "production" }
        };

        await containerClient.SetMetadataAsync(metadata);
        Console.WriteLine("Metadata has been set successfully.");

 
        BlobContainerProperties properties = await containerClient.GetPropertiesAsync();
        Console.WriteLine("Container Metadata:");
        foreach (var kvp in properties.Metadata)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}

```

crear contenendor 
![alt text](image-4.png)

y adicionar metadata al contenedor

![alt text](image-3.png)


subir la data del sitio estatico usando azcopy, el container por defecto es $web y lo detecta como variable así que para el storage account sas_token usar comilla sencilla para que no trate de reemplazar como variable de powershell o escapar en caso de linux.

```
./azcopy copy <localizacion_archivos> <storage_account_sas_token> --recursive=true
```
---
# cosmos DB create item in .net

![alt text](image-5.png)

herramienta de migracion de datos (AzureCosmosDB/data-migration-desktop-tool)


https://learn.microsoft.com/es-es/azure/cosmos-db/how-to-migrate-desktop-tool?tabs=azure-cli

https://github.com/AzureCosmosDB/data-migration-desktop-tool?tab=readme-ov-file#tutorial-json-to-cosmos-db-migration


descargar el ultimo release de la herramienta dmt

![alt text](image-171.png)

![alt text](image-172.png)

modificar el arhivo migration settings

con el conexion string, y el nombre de la base de datos creada usando el data explorer, y el contenedor

![alt text](image-173.png)

![alt text](image-174.png)

ejemplo de configuracion del archivo 'migrationsettings.json' tener encuenta el filepath no esta como en formato windows, tambien es mejor pegarlo en el mismo directorio para que funcione. airports.json fue usado.


```
{
  "Source": "json",
  "Sink": "cosmos-nosql",
  "SourceSettings": {
    "FilePath": "airports.json"
  },
  "SinkSettings": {
    "ConnectionString": "AccountEndpoint=....",
    "Database": "flightDetails",
    "Container": "airportCodes",
    "PartitionKeyPath": "/id",
    "RecreateContainer": true,
    "WriteMode": "Insert",
    "CreatedContainerMaxThroughput": 5000,
    "IsServerlessAccount": false
  }
}

```

ejecutar el comando ./dmt

![alt text](image-175.png)

importante el archivo json que se va a subir dentro del mismo directorio ( en este caso es el airports.json)

![alt text](image-176.png)

el formato debe ser algo como un array de json con el attributo id. tambien tener en cuenta las RU's (request units, se configuran a la creacion de la cuenta de cosmos)  si son muy bajas no sube.
```
[
    {
       "id":"00AK",
       "icao":"00AK",
       "iata":"",
       "name":"Lowell Field",
       "city":"Anchor Point",
       "state":"Alaska",
       "country":"US",
       "elevation":450,
       "lat":59.94919968,
       "lon":-151.695999146,
       "tz":"America/Anchorage"
    },
    {
        .....
    }
]
```

validacion de carga de datos en cosmosDB

![alt text](image-6.png)

crreeacion de un item nuevo en cosmos


![alt text](image-7.png)


colocar el endpoint y la key

![alt text](image-8.png)

storage procedures

![alt text](image-9.png)

![alt text](image-10.png)

![alt text](image-11.png)

![alt text](image-12.png)

![alt text](image-13.png)

![alt text](image-14.png)

![alt text](image-15.png)


## trieggers and user-defined functions

![alt text](image-16.png)

```
function pretrigger(){
    var item = getContext().getRequest().getBody();
    if(!item.country_code){
        item.country_code="unidentified";
    }

    item.timestamp = new Date().toISOString();

    getContext().getRequest().setBody(item)
}
```

![alt text](image-18.png)

![alt text](image-17.png)

![alt text](image-20.png)

tambien existen los pretriggers

![alt text](image-19.png)

change feed in azure cosmos: la idea es ejecutar codigo con cambios en algun documento o item en la bd.

![alt text](image-21.png)

![alt text](image-23.png)

![alt text](image-24.png)

![alt text](image-22.png)


# Azure container registries

```
dotnet --list-runtimes
mkdir App
cd App/
dotnet new webapp -n MyNewWebApp
cd MyNewWebApp/
dotnet restore MyNewWebApp.csproj 
```

```
cd MyNewWebApp/
cd Pages/
vi Index.cshtml
cat Index.cshtml
cd ../..
vi Dockerfile
```

```
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyNewWebApp/MyNewWebApp.csproj", "MyNewWebApp/"]
RUN dotnet restore "MyNewWebApp/MyNewWebApp.csproj"
COPY . .
WORKDIR "/src/MyNewWebApp"
RUN dotnet build "MyNewWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyNewWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyNewWebApp.dll"]
```

```
az acr build -r acr204052025 -g rg-registries -t webapp:v1 .

az acr buld -r <registry_name> -g <resource_group> -t <contaniername:TAG> <directory>
```

---
# azure container instances

![alt text](image-25.png)

azure container instances

![alt text](image-26.png)

![alt text](image-27.png)

![alt text](image-28.png)


azure container instances restart policies y variables de entorno


![alt text](image-29.png)

![alt text](image-30.png)

variables de entorno

![alt text](image-31.png)

para la configuaracion de container instance los datos se pueden tomar el contaner registry en la seccion de keys

![alt text](image-32.png)

un ejemplo de comando a usar, para crear una container instance junto con una imagen por defecto se muestra a continuacion

```
az container create -g rg-container-instance -n ac1-01 \
  --image acr204052025.azurecr.io/webapp:v1 \
  --registry-login-server acr204052025.azurecr.io \
  --registry-username acr204052025 \
  --registry-password 'xxxx' \
  --ports 8080 \
  --ip-address Public \
  --os-type Linux \
  --cpu 1 \
  --memory 2.0
```

![alt text](image-33.png)

![alt text](image-35.png)

![alt text](image-34.png)

montar file share en container instance

![alt text](image-36.png)

![alt text](image-37.png)

para utilizar un file share utilizar el siguiente archivo yaml tomado de https://learn.microsoft.com/en-us/azure/container-instances/container-instances-volume-azure-files,

importante cambiar la imagen, y el dnslabel con una valor aleatorio.

```
apiVersion: '2019-12-01'
location: eastus
name: file-share-demo
properties:
  containers:
  - name: hellofiles
    properties:
      environmentVariables: []
      image: mcr.microsoft.com/azuredocs/aci-hellofiles
      ports:
      - port: 80
      resources:
        requests:
          cpu: 1.0
          memoryInGB: 1.5
      volumeMounts:
      - mountPath: /aci/logs/
        name: filesharevolume
  osType: Linux
  restartPolicy: Always
  ipAddress:
    type: Public
    ports:
      - port: 80
    dnsNameLabel: aci-demo
  volumes:
  - name: filesharevolume
    azureFile:
      sharename: acishare
      storageAccountName: <Storage account name>
      storageAccountKey: <Storage account key>
tags: {}
type: Microsoft.ContainerInstance/containerGroups
```

desplegar usando este comando


```
az container create --resource-group myResourceGroup --file deploy-aci.yaml
```
---
# Azure container apps

![alt text](image-38.png)

push the container


create resource group

```
az group create --location westus --resource-group rgcontainerapp01
```
create enviroment container app

```
 az containerapp env create --name myEnvironment --resource-group rgcontainerapp01   --location westus 
```

create the container app

```
az containerapp create --name ariportsapi --resource-group rgcontainerapp01 --image acr204052025.azurecr.io/webapp02:v1 --cpu 0.5 --memory 1Gi --environment myEnvironment --ingress 'external' --target-port 8080 --registry-server acr204052025.azurecr.io --registry-username acr204052025 --registry-password 'xxx'

```

testing that api

![alt text](image-39.png)

authentication authorization

![alt text](image-40.png)

![alt text](image-41.png)

![alt text](image-42.png)

![alt text](image-43.png)

![alt text](image-44.png)

![alt text](image-45.png)

![alt text](image-46.png)

despues de las configuraciones anteriores se puede acceder a la api, pero se debe autenticar.

## managing revision and secrets

esta parte es parecido a los deployments slots de las webapp, se puede tener un biding para bases de datos o storages accounts, tambien se puede colocar porcentaje de redireccionamiento de trafico.

![alt text](image-47.png)

los secretos se manejan en esta pestaña

![alt text](image-48.png)

dapr integration

![alt text](image-49.png)

![alt text](image-50.png)


What is the purpose of the Distributed Application Runtime (Dapr) integration in Azure Container Apps? -> To provide a programming model for microservices


---
# Microsoft identity Platform

![alt text](image-51.png)


![alt text](image-52.png)

creacion de un secret en el app registation, para poder autenticar, ya la grantizacion de que el sp tenga accesos,

![alt text](image-53.png)

![alt text](image-54.png)

![alt text](image-55.png)

![alt text](image-56.png)

generacion de token auth para autenticacion service principal

![alt text](image-57.png)

```
curl --location 'https://login.microsoftonline.com/4066ac16-42bc-4a44-bb29-a0e8fbf7fbeb/oauth2/v2.0/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--header 'Cookie: fpc=AuV0L1SA7kFIg0-TJSsGUdwf5Y4PAQAAAMzFNt8OAAAA; stsservicecookie=estsfd; x-ms-gateway-slice=estsfd' \
--data-urlencode 'client_id=ca99dcbc-a432-4dd8-ad63-492c1e61dd2a' \
--data-urlencode 'client_secret=xxx' \
--data-urlencode 'grant_type=client_credentials' \
--data-urlencode 'scope=https://graph.microsoft.com/.default'
```

![alt text](image-58.png)

request para obtener los usuarios del tenant, el ejemplo de solicitud se saco de la pagina de microsoft https://developer.microsoft.com/en-us/graph/graph-explorer

![alt text](image-60.png)

![alt text](image-59.png)

se presentan error por falta de permisos, modificarlos en el app registrations

![alt text](image-61.png)

![alt text](image-62.png)

![alt text](image-63.png)

![alt text](image-64.png)

cambiando los niveles de accesos, a user read all en api permissions, y creando un token nuevo se logra la respuesta

![alt text](image-65.png)

se puede validar el token de sesion desde la pagina de decodificacion de microsoft https://jwt.ms/

![alt text](image-66.png)

conditional access

![alt text](image-67.png)

![alt text](image-68.png)


---

# MSAL (microsoft autentication library)

![alt text](image-69.png)

![alt text](image-70.png)

![alt text](image-71.png)

whit debbuging 

![alt text](image-72.png)

```
using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

class Program
{
    private static string tenantId = "";    
    private static string clientId = "";    
    private static string clientSecret = "";  
    private static string[] scopes = { "https://graph.microsoft.com/.default" }; 

    static async Task Main(string[] args)
    {
              IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .Build();

        try
        {
          
            var result = await app.AcquireTokenForClient(scopes)
                                  .ExecuteAsync();

 
            Console.WriteLine("Access Token:");
            Console.WriteLine(result.AccessToken);
        }
        catch (MsalServiceException ex)
        {
            // Handle the exception
            Console.WriteLine($"Error acquiring token: {ex.Message}");
        }
    }
}
```

creacion de service principal powershell
```
az ad sp create-for-rbac
```

---

# shared access signatures (sas)

![alt text](image-73.png)

![alt text](image-74.png)

![alt text](image-75.png)

![alt text](image-76.png)

arquitecturas de obtener sas tocken

![alt text](image-77.png)

![alt text](image-78.png)

## Stored Access Policies

![alt text](image-79.png)

![alt text](image-80.png)

---

# microsoft graph

![alt text](image-81.png)

![alt text](image-82.png)

![alt text](image-83.png)

![alt text](image-85.png)

![alt text](image-84.png)

![alt text](image-86.png)

habilitar en el service principal de public client flows

![alt text](image-87.png)

![alt text](image-88.png)

# Azure Key vault

![alt text](image-89.png)

![alt text](image-90.png)

# Managed identities

![alt text](image-91.png)

![alt text](image-92.png)

![alt text](image-93.png)

![alt text](image-94.png)

![alt text](image-95.png)

![alt text](image-96.png)

# App Configuration Service

configuracion del applicaciones sin necesidad de redesplegar.

![alt text](image-97.png)

![alt text](image-98.png)

![alt text](image-99.png)

![alt text](image-100.png)

generacion de connection string

![alt text](image-101.png)

generacion de valores en la App configuration

![alt text](image-102.png)

despues de iniciado el proyecto no nececita refrescar auto actualiza los cambios solo es conectarse al servicio.

![alt text](image-103.png)

![alt text](image-104.png)

---

# Api Management

adicionar scm a el dominio, el domino incluye la zona

![alt text](image-105.png)

![alt text](image-106.png)

politicas inbount(entrada), outbund(salida), backend(backend): (https://learn.microsoft.com/en-us/azure/api-management/rewrite-uri-policy)

![alt text](image-154.png)


```
<policies>
    <inbound>
        <base />
        <rewrite-uri template="/api/employee" />
        <set-backend-service id="apim-generated-policy" backend-id="WebApp_employee" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>

```


![alt text](image-155.png)

![alt text](image-156.png)

![alt text](image-157.png)

---
# Event grid


![alt text](image-107.png)

![alt text](image-108.png)

se crea un Event grid a nivel de topico de sistema

![alt text](image-109.png)

event grid handling of delivery errors

![alt text](image-110.png)


![alt text](image-111.png)

dar click en crear una subscripcion pero antes crear una logic app

![alt text](image-112.png)

![alt text](image-113.png)

![alt text](image-114.png)

![alt text](image-115.png)

generar la url en la cual la logic app va a ejecutarse.

![alt text](image-116.png)

despues de guardar ya se genera la url del webhook

![alt text](image-117.png)

generar un event subscripcion

![alt text](image-118.png)

![alt text](image-120.png)

pegar la url del webhook que se obtuvo para la logic app

![alt text](image-121.png)

![alt text](image-122.png)

![alt text](image-123.png)

## roles dentro de event grid

![alt text](image-124.png)

permisos necesarios para poder subscribirse a el event grid

![alt text](image-125.png)

las tres maneras de escuchar eventos de event grid

![alt text](image-126.png)

formas de validar subscripciones a enventos

![alt text](image-127.png)

# Event hubs

![alt text](image-128.png)

los casos de uso son para ingestion de grandes volumenes de datos

![alt text](image-129.png)

algunas de sus principales componentes son

![alt text](image-130.png)

event hub capture

![alt text](image-132.png)

capture window

![alt text](image-133.png)

scaling to througput units

![alt text](image-134.png)

change pricing tier

![alt text](image-135.png)

create an event hub

![alt text](image-136.png)

![alt text](image-138.png)

![alt text](image-139.png)

![alt text](image-140.png)

ahora genenerar eventos desde la webapp

![alt text](image-142.png)

seleccionar todas las metricas y crear el diagnostic

![alt text](image-143.png)

generar trafico

![alt text](image-144.png)

desde el event hub ya se pueden visualizar los eventos de la webapp

![alt text](image-145.png)

![alt text](image-146.png)

tambien guarda la data en el storage account

![alt text](image-147.png)

control de acceso a los eventos

![alt text](image-148.png)

![alt text](image-149.png)

autorization

![alt text](image-150.png)

se puede utilizando shared access policies

![alt text](image-151.png)

## event hub .net sdk

![alt text](image-152.png)

![alt text](image-153.png)


---

# messaging services

azure queues storage 

![alt text](image-158.png)

![alt text](image-159.png)

![alt text](image-160.png)

![alt text](image-161.png)

# redis cache

![alt text](image-162.png)

![alt text](image-163.png)


![alt text](image-164.png)

![alt text](image-165.png)

![alt text](image-166.png)

clientes de redis

https://github.com/qishibo/AnotherRedisDesktopManager

![alt text](image-170.png)



# azure CDN

![alt text](image-167.png)

![alt text](image-168.png)

![alt text](image-169.png)