# Kubernetes — FinovaTech

## Pré-requisitos

- kubectl configurado e apontando para o cluster
- Imagens Docker publicadas em um registry acessível pelo cluster

## Deploy

```bash
# Criar namespace
kubectl apply -f k8s/namespace.yaml

# Secrets e ConfigMap
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/configmap.yaml

# Serviços (ordem importa para dependências)
kubectl apply -f k8s/gateway-pagamento.yaml
kubectl apply -f k8s/processador-pagamento.yaml
kubectl apply -f k8s/deteccao-fraude.yaml
kubectl apply -f k8s/servico-notificacao.yaml
kubectl apply -f k8s/servico-catalogo.yaml
kubectl apply -f k8s/gateway-api.yaml
kubectl apply -f k8s/novamart-frontend.yaml
```

## HPA — ProcessadorPagamento

O `processador-pagamento` tem HPA configurado para escalar entre 2 e 10 réplicas com base em CPU (70%) e memória (80%).

```bash
# Verificar estado do HPA
kubectl get hpa -n finovatech

# Verificar pods
kubectl get pods -n finovatech
```
