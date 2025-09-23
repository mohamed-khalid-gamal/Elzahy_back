# Email Setup Guide for Elzahy Portfolio

This guide will help you configure email functionality in your Elzahy Portfolio application.

## Gmail SMTP Setup (Recommended)

### Step 1: Enable 2-Factor Authentication
1. Go to your [Google Account settings](https://myaccount.google.com/)
2. Navigate to "Security"
3. Enable "2-Step Verification" if not already enabled

### Step 2: Generate App Password
1. Go to your [Google Account settings](https://myaccount.google.com/)
2. Navigate to "Security" ? "2-Step Verification"
3. Scroll down to "App passwords"
4. Click "Select app" and choose "Mail"
5. Click "Select device" and choose "Other (custom name)"
6. Enter "Elzahy Portfolio" as the name
7. Click "Generate"
8. Copy the 16-character app password (it looks like: `abcd efgh ijkl mnop`)

### Step 3: Update Configuration
Replace the email section in your `appsettings.json`:

```json
{
  "Email": {
    "From": "your-gmail@gmail.com",
    "SmtpHost": "smtp.gmail.com", 
    "SmtpPort": "587",
    "SmtpUser": "your-gmail@gmail.com",
    "SmtpPassword": "your-16-char-app-password"
  }
}
```

**Important:** 
- Use your full Gmail address for both `From` and `SmtpUser`
- Use the 16-character app password, not your regular Gmail password
- Remove any spaces from the app password

## Alternative SMTP Providers

### SendGrid (Production Recommended)
```json
{
  "Email": {
    "From": "noreply@yourdomain.com",
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": "587", 
    "SmtpUser": "apikey",
    "SmtpPassword": "your-sendgrid-api-key"
  }
}
```

### Mailgun
```json
{
  "Email": {
    "From": "noreply@yourdomain.com",
    "SmtpHost": "smtp.mailgun.org",
    "SmtpPort": "587",
    "SmtpUser": "postmaster@mg.yourdomain.com",
    "SmtpPassword": "your-mailgun-password"
  }
}
```

### Development/Testing SMTP Servers

#### Papertrail (Free testing)
```json
{
  "Email": {
    "From": "test@example.com",
    "SmtpHost": "smtp.papertrail.com",
    "SmtpPort": "587",
    "SmtpUser": "",
    "SmtpPassword": ""
  }
}
```

#### MailHog (Local testing)
1. Install MailHog: `docker run -p 1025:1025 -p 8025:8025 mailhog/mailhog`
2. Configure:
```json
{
  "Email": {
    "From": "test@localhost",
    "SmtpHost": "localhost",
    "SmtpPort": "1025",
    "SmtpUser": "",
    "SmtpPassword": ""
  }
}
```
3. View emails at: http://localhost:8025

## Troubleshooting

### Common Error Messages

#### "Username and Password not accepted"
- **Cause:** Using regular Gmail password instead of app password
- **Solution:** Generate and use Gmail app password

#### "Authentication failed"
- **Cause:** Incorrect credentials or 2FA not enabled
- **Solution:** Verify email and app password are correct

#### "Connection timed out"
- **Cause:** Firewall or network issues
- **Solution:** Check network connectivity and firewall settings

#### "SSL/TLS errors"
- **Cause:** Incorrect port or security settings
- **Solution:** Use port 587 with StartTLS for Gmail

### Debug Steps

1. **Check Logs:** Look for detailed error messages in your application logs
2. **Test Connection:** Use a simple SMTP test tool to verify credentials
3. **Verify Configuration:** Ensure all email settings are correct
4. **Check Gmail Settings:** Verify 2FA and app passwords are set up

### Testing Email Functionality

You can test email by registering a new user or using the forgot password feature. Check the application logs for detailed error messages.

## Security Best Practices

1. **Never commit email passwords to source control**
2. **Use environment variables for production:**
   ```bash
   # Environment variables
   Email__SmtpUser=your-email@gmail.com
   Email__SmtpPassword=your-app-password
   ```

3. **Use different email accounts for different environments**
4. **Consider using dedicated email services for production**

## Email Templates

The application includes the following email templates:
- **Email Confirmation:** Sent after user registration
- **Password Reset:** Sent when user requests password reset  
- **Welcome Email:** Sent after email confirmation
- **Two-Factor Authentication:** Sent for 2FA codes

All templates are responsive HTML emails with professional styling.

## Production Considerations

For production deployment:
1. Use a dedicated email service (SendGrid, Mailgun, etc.)
2. Set up proper SPF, DKIM, and DMARC records
3. Use environment variables for all sensitive configuration
4. Monitor email delivery rates and bounces
5. Implement rate limiting for email sending

## Need Help?

If you continue to experience issues:
1. Check the application logs for detailed error messages
2. Verify your Gmail app password is correctly generated
3. Test with a simple SMTP client to isolate the issue
4. Consider using a different email provider for testing