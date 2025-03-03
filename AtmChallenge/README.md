# Encryption

1. Create the 32 bytes AES SECRET KEY for 256-bit encryption
openssl rand -base64 32

2. Creates the 16 bytes AES IV for 126-bit block size
openssl rand -base64 16

Configure the env variables into the docker-compose file.

# Local

init-script.sql contains some data, for testing.