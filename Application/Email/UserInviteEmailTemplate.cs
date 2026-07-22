using System.Net;

namespace RudFitAI.Application.Email;

public static class UserInviteEmailTemplate
{
    public static string BuildHtml(string inviteUrl, string toEmail)
    {
        string safeUrl = WebUtility.HtmlEncode(inviteUrl);
        string safeEmail = WebUtility.HtmlEncode(toEmail);

        return $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>Convite RudFit</title>
            </head>
            <body style="margin:0;padding:0;background-color:#F8FAFC;font-family:Arial,Helvetica,sans-serif;color:#0F172A;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#F8FAFC;padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background-color:#FFFFFF;border:1px solid #E2E8F0;border-radius:12px;overflow:hidden;">
                      <tr>
                        <td style="background-color:#16A34A;padding:24px 28px;">
                          <p style="margin:0;font-size:20px;font-weight:700;color:#FFFFFF;letter-spacing:-0.02em;">RudFit AI</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px;">
                          <h1 style="margin:0 0 12px;font-size:22px;line-height:1.3;font-weight:700;color:#0F172A;">Você foi convidado</h1>
                          <p style="margin:0 0 16px;font-size:15px;line-height:1.6;color:#64748B;">
                            Um administrador convidou <strong style="color:#0F172A;">{{safeEmail}}</strong> para acessar o RudFit AI.
                          </p>
                          <p style="margin:0 0 24px;font-size:15px;line-height:1.6;color:#64748B;">
                            Crie sua conta informando seu nome e uma senha. Depois você poderá completar seu perfil e começar a usar o sistema.
                          </p>
                          <table role="presentation" cellspacing="0" cellpadding="0">
                            <tr>
                              <td style="border-radius:8px;background-color:#16A34A;">
                                <a href="{{safeUrl}}" style="display:inline-block;padding:12px 22px;font-size:15px;font-weight:600;color:#FFFFFF;text-decoration:none;">
                                  Criar minha conta
                                </a>
                              </td>
                            </tr>
                          </table>
                          <p style="margin:24px 0 0;font-size:13px;line-height:1.5;color:#94A3B8;">
                            Se o botão não funcionar, copie e cole este link no navegador:<br />
                            <a href="{{safeUrl}}" style="color:#16A34A;word-break:break-all;">{{safeUrl}}</a>
                          </p>
                          <p style="margin:20px 0 0;font-size:12px;line-height:1.5;color:#94A3B8;">
                            Este convite expira em 7 dias. Se você não esperava este e-mail, pode ignorá-lo.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
