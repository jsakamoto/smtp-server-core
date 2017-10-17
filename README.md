# Toolbelt.Net.Smtp.SmtpServerCore [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Net.Smtp.SmtpServerCore.svg)](https://www.nuget.org/packages/Toolbelt.Net.Smtp.SmtpServerCore/) [![Build status](https://ci.appveyor.com/api/projects/status/1583deg0k8u7soef?svg=true)](https://ci.appveyor.com/project/jsakamoto/smtp-server-core)

## What's this?

This is a class library for .NET Core 2.0/.NET Framework 4.5.

This library implements SMTP (Simple Mail Transfer Protocol) conversation to build SMTP service.

This library provides the following features:

- Listen TCP port to connect SMTP client in background thread.
- Fire events when receive e-mail message via SMTP.
- SMTP Authentication (Plain, CRAM-MD5)

This library doesn't provides the following features:

- SSL/TLS connection
- "STARTTLS" command support

## License

[Mozilla Public License, version 2.0](LICENSE)