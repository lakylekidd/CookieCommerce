package nl.cookiecommerce.email;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.Map;
import org.keycloak.email.EmailException;
import org.keycloak.email.freemarker.FreeMarkerEmailTemplateProvider;
import org.keycloak.models.KeycloakSession;
import org.keycloak.util.JsonSerialization;

/** Keycloak retains action-token generation, validation, password changes and SMTP delivery. */
public final class CookieCommerceEmailTemplateProvider extends FreeMarkerEmailTemplateProvider {
    private static final HttpClient HTTP = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(3)).build();
    public CookieCommerceEmailTemplateProvider(KeycloakSession session) { super(session); }

    @Override
    protected EmailTemplate processTemplate(String subject, List<Object> subjectAttributes, String template, Map<String,Object> attributes) throws EmailException {
        String endpoint = System.getenv("COOKIECOMMERCE_TEMPLATE_URL");
        String key = System.getenv("COOKIECOMMERCE_SERVICE_KEY");
        String kind = switch (template) {
            case "password-reset.ftl" -> "password-reset";
            case "executeActions.ftl" -> user.getRequiredActionsStream().anyMatch(a -> a.equals("UPDATE_PASSWORD")) ? "account-invitation" : "password-reset";
            case "email-verification.ftl", "email-update-confirmation.ftl" -> "email-verification";
            default -> null;
        };
        if (kind != null && endpoint != null && key != null && attributes.get("link") != null) {
            try {
                Map<String,Object> payload = Map.of("templateKey", kind,
                    "customerName", user.getFirstName() == null ? "customer" : user.getFirstName(),
                    "actionUrl", attributes.get("link"), "expiryMinutes", attributes.get("linkExpiration"));
                HttpRequest request = HttpRequest.newBuilder(URI.create(endpoint)).timeout(Duration.ofSeconds(3))
                    .header("Content-Type", "application/json").header("X-Communications-Key", key)
                    .POST(HttpRequest.BodyPublishers.ofString(JsonSerialization.writeValueAsString(payload))).build();
                HttpResponse<String> response = HTTP.send(request, HttpResponse.BodyHandlers.ofString());
                if (response.statusCode() == 200) {
                    Map<?,?> rendered = JsonSerialization.readValue(response.body(), Map.class);
                    return new EmailTemplate((String)rendered.get("subject"), (String)rendered.get("textBody"), (String)rendered.get("htmlBody"));
                }
            } catch (InterruptedException interrupted) {
                Thread.currentThread().interrupt();
            } catch (Exception unavailable) {
                // Never log action URLs or message bodies. Recovery still uses the bundled Keycloak template.
            }
        }
        return super.processTemplate(subject, subjectAttributes, template, attributes);
    }
}
