using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    public void EnviarCodigo(string destino, string codigo)
    {
        var email = new MimeMessage();

        email.From.Add(MailboxAddress.Parse("SEU_EMAIL@gmail.com"));
        email.To.Add(MailboxAddress.Parse(destino));

        email.Subject = "Código de confirmação";

        email.Body = new TextPart("plain")
        {
            Text = $"Seu código é: {codigo}"
        };

        using var smtp = new SmtpClient();

        smtp.Connect("smtp.gmail.com", 587, false);

        // ⚠️ usar senha de app (não sua senha normal)
        smtp.Authenticate("habitflow.contato@gmail.com", "xndq gwku fmnj vjnx");

        smtp.Send(email);

        smtp.Disconnect(true);
    }
}