# Instalação do RabbitMQ 4.x no Debian 13 (Trixie)

## Objetivo

Este documento descreve o processo de instalação do RabbitMQ Server utilizando os repositórios oficiais da equipe do RabbitMQ, conforme realizado durante o curso **DotNetMessagingLab**.

---

# Ambiente

| Componente | Versão |
|------------|---------|
| Sistema Operacional | Debian GNU/Linux 13 (Trixie) |
| .NET SDK | 10.0.302 |
| RabbitMQ | 4.3.3 |
| Erlang/OTP | 27 |

---

# 1. Atualizar os repositórios do sistema

```bash
sudo apt update
```

---

# 2. Instalar as dependências

```bash
sudo apt install -y curl gnupg apt-transport-https
```

Esses pacotes permitem adicionar repositórios seguros utilizando HTTPS e chaves GPG.

---

# 3. Adicionar a chave oficial do RabbitMQ

```bash
curl -1sLf "https://keys.openpgp.org/vks/v1/by-fingerprint/0A9AF2115F4687BD29803A206B73A36E6026DFCA" \
| sudo gpg --dearmor \
| sudo tee /usr/share/keyrings/com.rabbitmq.team.gpg > /dev/null
```

A chave GPG é utilizada pelo APT para validar a autenticidade dos pacotes distribuídos pela equipe do RabbitMQ.

---

# 4. Adicionar os repositórios oficiais

Criar o arquivo:

```bash
sudo nano /etc/apt/sources.list.d/rabbitmq.list
```

Conteúdo do arquivo:

```text
## Modern Erlang

deb [arch=amd64 signed-by=/usr/share/keyrings/com.rabbitmq.team.gpg] https://deb1.rabbitmq.com/rabbitmq-erlang/debian/trixie trixie main
deb [arch=amd64 signed-by=/usr/share/keyrings/com.rabbitmq.team.gpg] https://deb2.rabbitmq.com/rabbitmq-erlang/debian/trixie trixie main

## RabbitMQ

deb [arch=amd64 signed-by=/usr/share/keyrings/com.rabbitmq.team.gpg] https://deb1.rabbitmq.com/rabbitmq-server/debian/trixie trixie main
deb [arch=amd64 signed-by=/usr/share/keyrings/com.rabbitmq.team.gpg] https://deb2.rabbitmq.com/rabbitmq-server/debian/trixie trixie main
```

---

# 5. Atualizar novamente os repositórios

```bash
sudo apt update
```

Verifique se:

- Não existem erros relacionados à chave GPG.
- Os repositórios do RabbitMQ foram carregados corretamente.

---

# 6. Instalar o Erlang

```bash
sudo apt install -y \
    erlang-base \
    erlang-asn1 \
    erlang-crypto \
    erlang-eldap \
    erlang-ftp \
    erlang-inets \
    erlang-mnesia \
    erlang-os-mon \
    erlang-parsetools \
    erlang-public-key \
    erlang-runtime-tools \
    erlang-snmp \
    erlang-ssl \
    erlang-syntax-tools \
    erlang-tftp \
    erlang-tools \
    erlang-xmerl
```

Optamos por instalar apenas os módulos necessários para o RabbitMQ, em vez do metapacote completo do Erlang.

---

# 7. Instalar o RabbitMQ Server

```bash
sudo apt install -y rabbitmq-server
```

Durante a instalação:

- O serviço é registrado automaticamente no `systemd`.
- Um usuário de sistema chamado `rabbitmq` é criado.
- O diretório de dados é preparado.

---

# 8. Verificar o serviço

```bash
systemctl status rabbitmq-server
```

Resultado esperado:

```text
Active: active (running)
```

---

# 9. Verificar a versão instalada

```bash
sudo rabbitmqctl version
```

Resultado obtido durante o curso:

```text
4.3.3
```

---

# 10. Verificar o status do broker

```bash
sudo rabbitmqctl status
```

Esse comando apresenta diversas informações úteis:

- Versão do RabbitMQ
- Versão do Erlang
- Tempo de atividade (uptime)
- Uso de memória
- Número de conexões
- Número de filas
- Plugins habilitados
- Diretório de dados
- Arquivos de log
- Listeners
- Virtual Hosts

É um dos comandos administrativos mais importantes do RabbitMQ.

---

# Interface Web

Na instalação realizada durante o curso, o plugin `rabbitmq_management` foi habilitado automaticamente.

A interface ficou disponível em:

```text
http://localhost:15672
```

Credenciais padrão (acesso local):

Usuário:

```text
guest
```

Senha:

```text
guest
```

---

# Portas utilizadas

| Porta | Finalidade |
|--------|------------|
| 5672 | Comunicação AMQP utilizada pelas aplicações |
| 15672 | Interface Web e HTTP API |
| 25672 | Comunicação entre nós do cluster |

---

# Diretórios importantes

Banco de dados interno (Mnesia):

```text
/var/lib/rabbitmq/mnesia
```

Arquivos de log:

```text
/var/log/rabbitmq/
```

---

# Plugins identificados

Durante a instalação foram carregados os seguintes plugins:

- rabbitmq_management
- rabbitmq_management_agent
- rabbitmq_web_dispatch
- amqp_client
- cowboy
- oauth2_client

---

# Conceitos aprendidos

Ao final da instalação foram compreendidos os seguintes conceitos:

- RabbitMQ é um Message Broker.
- RabbitMQ é desenvolvido em Erlang.
- O RabbitMQ é executado sobre a máquina virtual BEAM.
- O banco interno utilizado é o Mnesia.
- A interface administrativa é fornecida pelo plugin `rabbitmq_management`.
- O protocolo principal utilizado pelo RabbitMQ é o AMQP.
- O serviço é gerenciado pelo `systemd`.

---

# Observações

- Foi utilizada a documentação oficial do RabbitMQ.
- Foram utilizados os repositórios oficiais para Debian 13 (Trixie).
- O ambiente foi preparado para fins de estudo utilizando .NET 10 e RabbitMQ 4.x.
- O broker iniciou corretamente e ficou acessível pela interface web.

---

# Próximos passos

Na sequência do curso serão abordados os seguintes tópicos:

- Explorando a interface administrativa.
- Exchanges.
- Queues.
- Bindings.
- Publicação de mensagens.
- Consumo de mensagens.
- Integração do RabbitMQ com aplicações .NET 10.