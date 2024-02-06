#!/bin/bash
if [ ! -f /vault/initialized ]
then
    apt update
    apt install net-tools libcap2-bin openssl apt-utils lsb-release wget gpg -y
    dpkg -i /vault_1.15.4-1_amd64.deb
fi
cd /vault
export ip=$(ifconfig | grep -oE 'inet (addr:)?([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)' | grep -v '127.0.0.1' | awk '{print $2}')
cat <<EOF > req.conf
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no
[req_distinguished_name]
C = PL
ST = Wielkopolskie
L = Poznan
O = DBG
OU = IT
CN = 127.0.0.1
[v3_req]
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @alt_names
[alt_names]
IP.1 = 127.0.0.1
EOF
if [ ! -f /vault/initialized ]
then
	openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -keyout key.pem -out cert.pem -config req.conf 2> /dev/null
    sed -i 's|tls_cert_file = "/opt/vault/tls/tls.crt"|tls_cert_file = "/vault/cert.pem"|' /etc/vault.d/vault.hcl
    sed -i 's|tls_key_file  = "/opt/vault/tls/tls.key"|tls_key_file  = "/vault/key.pem"|' /etc/vault.d/vault.hcl
    sed -i 's|path = "/opt/vault/data"|path = "/vault"|' /etc/vault.d/vault.hcl
    sed -i 's|#disable_mlock = true|disable_mlock = true|' /etc/vault.d/vault.hcl
    chown vault:vault /vault/key.pem /vault/cert.pem /usr/bin/vault
fi
export VAULT_ADDR="https://127.0.0.1:8200"
/usr/bin/vault server -config=/etc/vault.d/vault.hcl -ca-cert=/vault/cert.pem 2>&1 &
sleep 10
if [ ! -f /vault/initialized ]
then
    prompt=$(vault operator init -key-shares=3 -key-threshold=2 -ca-cert=/vault/cert.pem)
    unseal_key_1=$(echo $prompt | grep -o 'Unseal Key 1: [^ ]*' | cut -d' ' -f4)
    unseal_key_2=$(echo $prompt | grep -o 'Unseal Key 2: [^ ]*' | cut -d' ' -f4)
    unseal_key_3=$(echo $prompt | grep -o 'Unseal Key 3: [^ ]*' | cut -d' ' -f4)
    root_token=$(echo $prompt |  grep -o 'Initial Root Token: [^ ]*' | cut -d' ' -f4)
    echo $root_token>/vault/rt
    echo $unseal_key_1>/vault/uk1
    echo $unseal_key_2>/vault/uk2
    echo $unseal_key_3>/vault/uk3
fi
cat /vault/uk1 | xargs vault operator unseal -ca-cert=/vault/cert.pem
cat /vault/uk2 | xargs vault operator unseal -ca-cert=/vault/cert.pem
if [ ! -f /vault/initialized ]
then
    VAULT_TOKEN=$root_token vault secrets enable -ca-cert=/vault/cert.pem -path=DBG -version=2 kv
    touch /vault/initialized
fi
tail -f /dev/null