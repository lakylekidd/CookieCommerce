FROM maven:3.9.11-eclipse-temurin-21 AS provider
WORKDIR /build
COPY pom.xml .
COPY src ./src
RUN mvn -B -q package

FROM quay.io/keycloak/keycloak:26.4.7
COPY --from=provider --chown=keycloak:keycloak /build/target/cookiecommerce-email-templates-1.0.0.jar /opt/keycloak/providers/
ENV KC_SPI_EMAIL_TEMPLATE__PROVIDER=cookiecommerce
