NAMESPACE := kafka-cart-lab
MANIFEST := k8s/minikube.yaml
PORTFWD_PID_DIR := .portforward-pids

.PHONY: help minikube-start build-images deploy teardown status url-summary minikube-url forwardport start-portforwards stop-portforwards

help:
	@echo "Available targets:"
	@echo "  make deploy        - Build images, deploy, and auto start port-forwards"
	@echo "  make teardown      - Remove resources and auto stop port-forwards"
	@echo "  make forwardport   - Alias for starting background port-forwards"
	@echo "  make url-summary   - Print useful local and cluster URLs"
	@echo "  make status        - Show pods and services"

minikube-start:
	minikube start

build-images:
	minikube image build -t gateway:local -f src/ApiGateway/Dockerfile .
	minikube image build -t productservice:local -f src/Services/ProductService/Dockerfile .
	minikube image build -t cartservice:local -f src/Services/CartService/Dockerfile .
	minikube image build -t inventoryservice:local -f src/Services/InventoryService/Dockerfile .
	minikube image build -t paymentservice:local -f src/Services/PaymentService/Dockerfile .
	minikube image build -t orderservice:local -f src/Services/OrderService/Dockerfile .

deploy: build-images
	kubectl apply -f $(MANIFEST)
	$(MAKE) start-portforwards
	@echo "Deployment submitted and port-forwards started."

teardown:
	$(MAKE) stop-portforwards
	kubectl delete -f $(MANIFEST) --ignore-not-found=true
	@echo "Resources removed and port-forwards stopped."

status:
	kubectl get pods -n $(NAMESPACE)
	kubectl get svc -n $(NAMESPACE)

minikube-url:
	minikube service gateway -n $(NAMESPACE) --url

forwardport: start-portforwards

start-portforwards:
	@mkdir -p $(PORTFWD_PID_DIR)
	@echo "Starting background port-forwards..."
	@sh -c 'kubectl port-forward svc/kafdrop 9000:9000 -n $(NAMESPACE) > $(PORTFWD_PID_DIR)/kafdrop.log 2>&1 & echo $$! > $(PORTFWD_PID_DIR)/kafdrop.pid'
	@sh -c 'kubectl port-forward svc/jaeger 16686:16686 -n $(NAMESPACE) > $(PORTFWD_PID_DIR)/jaeger.log 2>&1 & echo $$! > $(PORTFWD_PID_DIR)/jaeger.pid'
	@sh -c 'kubectl port-forward svc/seq 5341:80 -n $(NAMESPACE) > $(PORTFWD_PID_DIR)/seq.log 2>&1 & echo $$! > $(PORTFWD_PID_DIR)/seq.pid'
	@echo "Port-forwards started:"
	@echo "  Kafdrop -> http://localhost:9000"
	@echo "  Jaeger  -> http://localhost:16686"
	@echo "  Seq     -> http://localhost:5341"

stop-portforwards:
	@echo "Stopping background port-forwards (if running)..."
	@sh -c 'if [ -f "$(PORTFWD_PID_DIR)/kafdrop.pid" ]; then kill `cat $(PORTFWD_PID_DIR)/kafdrop.pid` 2>/dev/null || true; rm -f $(PORTFWD_PID_DIR)/kafdrop.pid; fi'
	@sh -c 'if [ -f "$(PORTFWD_PID_DIR)/jaeger.pid" ]; then kill `cat $(PORTFWD_PID_DIR)/jaeger.pid` 2>/dev/null || true; rm -f $(PORTFWD_PID_DIR)/jaeger.pid; fi'
	@sh -c 'if [ -f "$(PORTFWD_PID_DIR)/seq.pid" ]; then kill `cat $(PORTFWD_PID_DIR)/seq.pid` 2>/dev/null || true; rm -f $(PORTFWD_PID_DIR)/seq.pid; fi'
	@echo "Port-forward processes stopped."

url-summary:
	@echo "Gateway URL (Minikube service):"
	@minikube service gateway -n $(NAMESPACE) --url
	@echo ""
	@echo "Observability/Tool URLs (auto-forwarded by 'make deploy'):"
	@echo "Kafdrop: http://localhost:9000"
	@echo "Jaeger:  http://localhost:16686"
	@echo "Seq:     http://localhost:5341"
	@echo "ProductService health (inside cluster): http://productservice.$(NAMESPACE).svc.cluster.local:8080/health"
