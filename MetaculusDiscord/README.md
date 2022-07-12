# How to deploy this
1. build the image and push it to GCR

```
HASH=$(git log --pretty=format:%h -n 1)
IMAGE_NAME="eu.gcr.io/root-quasar-329715/metaculusdiscord:${HASH}"
docker build -t $IMAGE_NAME .
docker push $IMAGE_NAME
```

2. deploy the `kubectl apply -f secrets.yaml` and then use `kubectl edit secrets discord-bot-secrets` and manually change the `DISCORD_TOKEN` in it (you need to get it from elsewhere) - this steps needs to be done only once during the first deploy (unless you changed the token, of course). Use `echo -n "{original-token-here}" | base64` to get the string.
3. go to `deployment.yaml` and manually add `$IMAGE_NAME` from the above to `spec.template.spec.containers.image` 
4. deploy the new bot: `kubectl apply -f deployment.yaml`


# DB Setup (needs to be done once)
1. Manually go to https://console.cloud.google.com/sql/instances/metac-db/databases?project=root-quasar-329715 and create a new database `metaculus_discord_bot`
2. go to https://console.cloud.google.com/sql/instances/metac-db/users?project=root-quasar-329715 and create a new user `metaculus_discord_user` with some strong password
3. set this password in secrets similarly to the `DISCORD_TOKEN`
4. `gcloud sql connect metac-db --database metaculus_discord_user  --user metaculus_discord_bot`
5. run SQL content of `init-db.sh` to create the tables (without creating the user)