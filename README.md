# RabbitMQ sidecar-based consuming app example 
This is an example app and setup for consuming RabbitMQ messages using a sidecar in K8s

## Structure
* `my-app/`: a simple Node app that expose http server, and using socket.io to transfer messages between all clients, and also expose the same functionality via http route (`/message`)
* `RabbitMQ.Consumer/`: a simple .NET app that consumes from a RabbitMQ server and dispatch dispatch each message to a configured http server
* `docker/`
  * `/MyApp.Dockerfile` builds `my-app` image
  * `/RabbitMQ.Consumer.Dockerfile` builds `RabbitMQ.Consumer` image
* `k8s/` - yaml files for deploying the above into your cluster (detailed below)


## Prerequisites
* docker
* minikube
* kubectl

## Init your env
Start minikube:
```
minukube start
```
and create your namespace:
```
kubectl create ns my-app
```

## Build and load the images into minikube
```
minikube image build -t my-app:latest -f docker/MyApp.Dockerfile . 
minikube image build -t rabbitmq-consumer:latest -f docker/RabbitMQ.Consumer.Dockerfile . 
```
For each change in the services' code you'll need to re-run this step and restrat the relevant pods

## Expose your internal services from minikube
The following command will automatically expose all services of type `LoadBalancer` from your minikube cluster
```
minikube tunnel
```
Note this is a blocking call, run this in a new terminal/shell.

## Install the app on your minikube cluster
RabbitMQ Server:
```
kubectl -n my-app apply -f k8s/rabbitmq.yaml
```
this exposes by default both RabbitMQ server on port `5672` and a RabbitMQ management portal on port `15672` (for simplicitly I used the default username and password: `guest` for both)
  
`my-app`:
```
kubectl -n my-app apply -f k8s/my-app.yaml
```

you should now see both apps running (by running `kubectl -n my-app get pods`):
```
NAME                        READY   STATUS    RESTARTS   AGE
my-app-5dc96989c6-rgwp6     1/1     Running   0          29h
rabbitmq-54f95b8b4b-69qnx   1/1     Running   0          29h
```
also, you can see that you have both services configured (by running `kubectl -n my-app get svc`):
```
NAME                  TYPE           CLUSTER-IP       EXTERNAL-IP   PORT(S)           AGE
my-app                LoadBalancer   x.x.x.x          127.0.0.1     8080:31733/TCP    29h
rabbitmq              ClusterIP      x.x.x.x          <none>        5672/TCP          29h
rabbitmq-management   LoadBalancer   x.x.x.x          127.0.0.1     15672:31811/TCP   29h
```
you can see that we exposed only the RabbitMQ management portal and not the RabbitMQ server itself
you should now see that in the `minikube tunnel` terminal you have two service tunnels started:
```
> minikube tunnel
🏃  Starting tunnel for service my-app.
🏃  Starting tunnel for service rabbitmq-management.
```

## Patch `my-app` to start consuming from RabbitMQ
First let's add our queues configurations:
```
kubectl -n my-app apply -f k8s/queues.yaml
```
you should now see the config map applied in your namespace (by running `kubectl -n my-app get cm`):
```
NAME                 DATA   AGE
queues-definitions   1      29h
```
then, let's re-install `my-app`, but now with the `RabbitMQ.Consumer` sidecar container:
```
kubectl -n my-app apply -f k8s/my-app-sidecar.yaml
```
after this you should see the following output by running `kubectl -n my-app get pods`
```
NAME                        READY   STATUS    RESTARTS   AGE
my-app-5dc96989c6-rgwp6     2/2     Running   0          29h
rabbitmq-54f95b8b4b-69qnx   1/1     Running   0          29h
```
(see the `2/2` ready containers in `my-app-<>` pod)

## Testing the integration
Open my-app website by browsing to `localhost:8080`, you should see a blank chat session.  
You can now enter to RabbitMQ management portal through `localhost:15672` (and enter `guest` for both user and password).  
Go to `Queues` tab, and enter the newly created queue (`my-cool-queue`), publish a new message and you should now see this message in your chat session, something like:
```
{"QueueMessage":{"QueueName":"my-cool-queue","Data":"hi","Priority":0,"RetryCount":0,"Headers":null,"ConsumeId":"c790d53b-dab7-4583-954e-029c61349b1b"},"TimeoutInSeconds":0}
```


### That's it!
Feel free to play with it and change the endpoint to present the QueueMessage.Data in the chat instead of the whole payload.  
I would appreciate any feedback, enjoy :)