# Kafka Cart Lab

Implementation foundation generated from the attached blueprint:

- Docker Compose infrastructure (Kafka, Kafdrop, Redis, SQL Server, Seq, Jaeger)
- Shared .NET building blocks (`Common`, `EventBus`, `Outbox`, `Observability`)
- First functional microservice: `ProductService` with SQL-backed CRUD
- Additional service containers scaffolded (`CartService`, `InventoryService`, `PaymentService`, `OrderService`, `ApiGateway`)

## Project Structure

```text
src/
  ApiGateway/
  BuildingBlocks/
    Common/
    EventBus/
    Outbox/
    Observability/
  Services/
    ProductService/
    CartService/
    InventoryService/
    PaymentService/
    OrderService/
```

## Deployment Options

This project supports two deployment modes:

- Docker Compose (single-machine local stack)
- Minikube (Kubernetes local cluster)

## Docker Compose Deployment

1. Build solution:

```bash
dotnet build KafkaCartLab.sln
```

2. Start full stack:

```bash
docker compose up --build
```

3. Useful endpoints:

- API Gateway: `http://localhost:5000/health`
- ProductService: `http://localhost:5001/health`
- Kafdrop UI: `http://localhost:9000`
- Jaeger UI: `http://localhost:16686`
- Seq UI: `http://localhost:5341`

## Product API

- `GET /api/products`
- `POST /api/products`

## Minikube Deployment

Prerequisites:

- `minikube`
- `kubectl`
- `make`

1. Start Minikube:

```bash
make minikube-start
```

2. Deploy to Minikube (build images + apply manifest + auto port-forward):

```bash
make deploy
```

3. Check rollout:

```bash
make status
```

4. Print URL summary:

```bash
make url-summary
```

5. Tear down (also stops port-forward processes):

```bash
make teardown
```

### Minikube URLs

- Gateway URL: run `make minikube-url`
- Gateway health: `<gateway-url-from-command>/health`
- ProductService (cluster-internal): `http://productservice.kafka-cart-lab.svc.cluster.local:8080/health`

### Auto Port-Forwarded URLs (from `make deploy`)

- Kafdrop UI: `http://localhost:9000`
- Jaeger UI: `http://localhost:16686`
- Seq UI: `http://localhost:5341`

## Troubleshooting

- **Minikube context not active**
  - Run `kubectl config current-context`
  - If needed, switch with `kubectl config use-context minikube`
- **Port-forward does not start**
  - Check services: `kubectl get svc -n kafka-cart-lab`
  - Restart forwards: `make teardown` then `make deploy`
- **Port already in use (9000/16686/5341)**
  - Stop conflicting process or change local forwarding ports in `Makefile`
- **Images not found in cluster**
  - Rebuild with `make build-images` and redeploy with `make deploy`
- **Pods not becoming ready**
  - Check details: `kubectl get pods -n kafka-cart-lab`
  - Inspect logs: `kubectl logs <pod-name> -n kafka-cart-lab`
