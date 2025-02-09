# Instruções para Deploy do MyFood WebAPI Orders (Docker Local)

## Construção e Publicação da Imagem Docker
Execute os seguintes comandos para construir e publicar a imagem Docker da aplicação:

```sh
docker build -t themisterbondy/myfood-orders-webapi -f src/Postech.Fiap.Orders.WebApi/Dockerfile .

docker push themisterbondy/myfood-orders-webapi
```

## Criação do Namespace
Crie o namespace para o MyFood WebAPI:

```sh
kubectl create namespace myfood-namespace
```

## ConfigMap para Banco de Dados do MyFood WebAPI
Crie um ConfigMap para armazenar as credenciais de conexão com o banco de dados:

```sh
kubectl create configmap myfood-db-orders-config --namespace=myfood-namespace --from-literal=ConnectionStrings__SQLConnection="Host=host.docker.internal;Database=myfooddb-orders;Username=myfooduser;Password=blueScreen@666"
```

## ConfigMap para Azure Storage Account
Crie um ConfigMap para armazenar as informações de conexão com a conta de armazenamento Azure:

```sh
kubectl create configmap myfood-storage-account-config --namespace=myfood-namespace --from-literal=AzureStorageSettings__ConnectionString="UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://host.docker.internal"
```

## Instalação do MyFood WebAPI com Helm
Para instalar a aplicação no Kubernetes usando Helm, execute:

```sh
helm install myfood-orders-webapi .\charts\webapi\ --namespace myfood-namespace
```

## Atualização do MyFood WebAPI com Helm
Caso precise atualizar a aplicação, utilize o comando abaixo:

```sh
helm upgrade myfood-orders-webapi .\charts\webapi\ --namespace myfood-namespace
```

## Redirecionamento de Porta no MyFood WebAPI
Utilize o seguinte comando para redirecionar a porta e acessar o serviço localmente:

```sh
kubectl port-forward svc/myfood-orders-webapi 60075:80 -n myfood-namespace
```

## Monitoramento e Depuração

### Obter a Lista de Pods
Monitore os Pods do namespace com o comando:

```sh
kubectl get pods --namespace myfood-namespace --watch
```

### Acessar um Pod Interativamente
Entre em um Pod da aplicação de forma interativa usando o comando:

```sh
kubectl exec -it myfood-orders-webapi-75ccdb8997-dg624 -- /bin/sh
```

### Descrever um Pod
Para obter detalhes de um Pod específico no Kubernetes, execute:

```sh
kubectl describe pod myfood-orders-webapi-64d46cb67-nkbzl --namespace myfood-namespace
```

### Verificar Logs dos Pods
Confira os logs dos Pods com os comandos abaixo:

```sh
kubectl logs myfood-orders-webapi-64d46cb67-nkbzl --namespace myfood-namespace
```

## Remover Recursos do MyFood WebAPI

### Remover o Deployment
Para remover apenas o deployment do MyFood WebAPI, utilize:

```sh
kubectl delete deployment myfood-orders-webapi --namespace myfood-namespace
```

### Remover o Namespace
Caso precise excluir o namespace e todos os seus recursos, use:

```sh
kubectl delete namespace myfood-namespace
```