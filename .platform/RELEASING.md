# Releasing CookieCommerce

Merging a merge request into `master` runs verification. It does not create a
version tag or deploy the application. The release operator chooses each stable
version. Staging is disabled for this project, and production deployment needs
a separate approval for the exact homelab release plan.

## One-time platform setup

1. Complete the private homelab platform configuration for Infisical, Coolify,
   and Nginx Proxy Manager. Manage DNS in Cloudflare for `tigervancookie.nl`
   and `login.tigervancookie.nl`. Keep credentials in Infisical and the
   protected MCP host environment, not in this repository.
2. Configure the GitLab project and protect `v*` tags so only Maintainers can
   create release tags. Confirm the target runner can build and push rootless
   BuildKit images to the GitLab registry.
3. Use the manifest's `coolify` secret mode: the homelab controller reads only
   the declared Infisical keys and writes them as runtime variables on its
   owned Coolify applications. Give the controller scoped access to the
   application secret project. Its own bootstrap credentials stay separate.
   Configure and test database, RabbitMQ, and volume backups before storing
   production data.
4. Run the homelab bootstrap plan for this repository and review its proposed
   Coolify applications and Nginx Proxy Manager routes for `tigervancookie.nl`
   and `login.tigervancookie.nl`.
   The APIs remain internal. A first protected-tag build is needed before the
   applications can be pinned to image digests.
5. After the first protected-tag build, use the homelab infrastructure plan to
   configure and start the five PostgreSQL databases, RabbitMQ, and Keycloak
   separately. Verify their health and the Keycloak realm discovery endpoint
   before promoting the application images. This setup does not run as part
   of later application releases.

## Each release

1. Merge the reviewed changes into `master` and wait for its GitLab pipeline
   to pass.
2. Choose an unused version in `vMAJOR.MINOR.PATCH` form. The old `v0.1` tag
   is not a valid stable release version for this pipeline.
3. Fetch `master` and tags. Create an annotated tag on the exact tested
   `origin/master` commit, then push that tag to GitLab:

   ```powershell
   $version = Read-Host "Stable version (vMAJOR.MINOR.PATCH)"
   if ($version -cnotmatch '^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') { throw "Invalid release version" }
   git fetch origin master --tags
   git log -1 --format="%h %s" origin/master
   git tag -a $version origin/master -m "Release $version"
   git push origin "refs/tags/$version"
   ```

4. The protected-tag pipeline validates the tag, runs verification, builds the
   four service images and the custom Keycloak image, and saves their immutable
   digest receipts. It does not deploy to Coolify.
5. Use homelab `release.validate` for the chosen version. Review the
   `release.promoteProduction` dry-run, including the exact image digests,
   applications, volumes, and public route. A separate operator issues the
   plan-bound approval. Apply that plan with its approval ID, then run
   `release.verify` until the applications and HTTPS route are healthy.

The homelab MCP server runs on the operator machine over stdio. GitLab CI does
not call it for production promotion. Do not put MCP credentials or production
approval material in CI variables.
