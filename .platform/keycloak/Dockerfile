FROM docker.io/library/maven:3.9.11-eclipse-temurin-21@sha256:6fdc855a6ed81d288ca7ca37ac6ff5e9308b612485c0801d70b25a858c83d237 AS provider
WORKDIR /build
COPY src/CookieCommerce.KeycloakEmailProvider/pom.xml .
COPY src/CookieCommerce.KeycloakEmailProvider/src ./src
RUN mvn -B -q package

FROM quay.io/keycloak/keycloak:26.4.7@sha256:9409c59bdfb65dbffa20b11e6f18b8abb9281d480c7ca402f51ed3d5977e6007
COPY --from=provider --chown=keycloak:keycloak /build/target/cookiecommerce-email-templates-1.0.0.jar /opt/keycloak/providers/
COPY --chown=keycloak:keycloak .platform/keycloak/cookiecommerce-realm.json /opt/keycloak/data/import/cookiecommerce-realm.json
ENV KC_SPI_EMAIL_TEMPLATE__PROVIDER=cookiecommerce
