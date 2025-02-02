# Instruções para Deploy do MyFood WebAPI

## Construção e Publicação da Imagem Docker
```sh
docker build -t themisterbondy/myfood-orders-webapi -f src/Postech.Fiap.Orders.WebApi/Dockerfile .

docker push themisterbondy/myfood-orders-webapi
```

##  Config Map para banco de dados do MyFood WebAPI
```sh
kubectl create configmap myfood-db-orders-config --namespace=myfood-namespace --from-literal=ConnectionStrings__SQLConnection="Host=host.docker.internal;Database=myfooddb-orders;Username=myfooduser;Password=blueScreen@666"
```

##  Config Map para Azure Storage Account do MyFood WebAPI 
```sh
kubectl create configmap myfood-storage-account-config --namespace=myfood-namespace --from-literal=AzureStorageSettings__ConnectionString="UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://host.docker.internal"
```

## Instalação  do MyFood WebAPI com Helm
```sh
helm install myfood-orders-webapi .\charts\webapi\ --namespace myfood-namespace
```

## Atualização do MyFood WebAPI com Helm
```sh
helm upgrade myfood-orders-webapi .\charts\webapi\ --namespace myfood-namespace
```

## Monitoramento e Depuração

### Obter a Lista de Pods
```sh
kubectl get pods --namespace myfood-namespace --watch
```

### Acessar um Pod Interativamente
```sh
kubectl exec -it myfood-orders-webapi-75ccdb8997-dg624 -- /bin/sh
```

### Descrever um Pod
```sh
kubectl describe pod myfood-orders-webapi-64d46cb67-nkbzl --namespace myfood-namespace
```

### Verificar Logs dos Pods
```sh
kubectl logs myfood-orders-webapi-64d46cb67-nkbzl --namespace myfood-namespace

kubectl logs myfood-orders-webapi-57944c9cf-qr595 --namespace myfood-namespace
```

## Remover o Namespace do MyFood
```sh
kubectl delete namespace myfood-namespace
```