# Encryption

Once the credit card and pin are provided, those are encrypted using RSA 2048 key/pair.

The card numbers are never or pin are stored encrypted.

The data required to access the endpoints are also encrypted inside the JWT token, if anyone opens the token, will
not be able to get any sensitive information.

A self-signed certificate is provided as an example, but this must be replaced for prod environments.
Those certificates are under the certificates folder

For Custom keys please override the following path with your certificates in the docker-compose

    volumes:
      - ./custom_certificates:/app/certificates

# Local

Set the following env variables.

export PUBLIC_KEY_PATH="$(pwd)/certificates/public_key.pem
export PRIVATE_KEY_PATH="$(pwd)/certificates/private_key.pem
