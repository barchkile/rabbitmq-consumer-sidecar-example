# RabbitMQ sidecar-based consuming app example 
This is an example app and setup for consuming RabbitMQ messages using a sidecar in K8s

# Prerequisites
* docker
* minikube
* kubectl


## Init your minikube cluster
run the following:  
```
minikube start
minikube image build -t my-app:latest -f MyApp.Dockerfile . 
minikube image build -t rabbitmq-consumer:latest -f RabbitMQ.Consumer.Dockerfile . 
minikube tunnel
```

## Install the app in your namespace
run the following:  
```
kubectl create ns my-app
kubectl -n my-app apply -f rabbitmq.yaml
kubectl -n my-app apply -f my-app.yaml
```

## Patch the installation with the queues enhancment
run the following:  
```
kubectl -n my-app apply -f queues.yaml
kubectl -n my-app apply -f my-app-sidecar.yaml
```

