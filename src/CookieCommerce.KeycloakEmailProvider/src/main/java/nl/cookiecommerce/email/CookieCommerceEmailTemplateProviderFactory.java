package nl.cookiecommerce.email;

import org.keycloak.email.EmailTemplateProvider;
import org.keycloak.email.freemarker.FreeMarkerEmailTemplateProviderFactory;
import org.keycloak.models.KeycloakSession;

public final class CookieCommerceEmailTemplateProviderFactory extends FreeMarkerEmailTemplateProviderFactory {
    @Override public EmailTemplateProvider create(KeycloakSession session) { return new CookieCommerceEmailTemplateProvider(session); }
    @Override public String getId() { return "cookiecommerce"; }
    @Override public int order() { return 100; }
}
