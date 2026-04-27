# Minikube Deployment

This folder contains a single Kubernetes manifest for local Minikube deployment.

- Namespace: `kafka-cart-lab`
- Manifest: `k8s/minikube.yaml`

## 1) Start Minikube

```bash
minikube start
```

## 2) Deploy with Make

From repository root:

```bash
make deploy
```

This builds all local service images into Minikube and applies `k8s/minikube.yaml`.

## 3) Verify rollout

```bash
make status
```

## 4) URL summary

```bash
make url-summary
```

## 5) Forward ports (tooling + observability)

```bash
make forwardport
```

Then run each printed `kubectl port-forward` command in separate terminals.

## Manual equivalents (without Make)

### Build service images into Minikube

Use Minikube Docker daemon so cluster can pull local images:

```bash
minikube image build -t gateway:local -f src/ApiGateway/Dockerfile .
minikube image build -t productservice:local -f src/Services/ProductService/Dockerfile .
minikube image build -t cartservice:local -f src/Services/CartService/Dockerfile .
minikube image build -t inventoryservice:local -f src/Services/InventoryService/Dockerfile .
minikube image build -t paymentservice:local -f src/Services/PaymentService/Dockerfile .
minikube image build -t orderservice:local -f src/Services/OrderService/Dockerfile .
```

### Apply resources

```bash
kubectl apply -f k8s/minikube.yaml
```

### Verify rollout

```bash
kubectl get pods -n kafka-cart-lab
kubectl get svc -n kafka-cart-lab
```

### Access endpoints

- Gateway is exposed as NodePort:

```bash
minikube service gateway -n kafka-cart-lab --url
```

- Optional port-forward for observability and tooling:

```bash
kubectl port-forward svc/kafdrop 9000:9000 -n kafka-cart-lab
kubectl port-forward svc/jaeger 16686:16686 -n kafka-cart-lab
kubectl port-forward svc/seq 5341:80 -n kafka-cart-lab
```
