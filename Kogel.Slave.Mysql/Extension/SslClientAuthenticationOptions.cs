using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kogel.Slave.Mysql.Extension
{
#if NETSTANDARD2_0
	public class SslClientAuthenticationOptions
	{
		private EncryptionPolicy _encryptionPolicy;

		private X509RevocationMode _checkCertificateRevocation;

		private SslProtocols _enabledSslProtocols;

		private bool _allowRenegotiation = true;

		public bool AllowRenegotiation
		{
			get
			{
				return _allowRenegotiation;
			}
			set
			{
				_allowRenegotiation = value;
			}
		}

		public LocalCertificateSelectionCallback LocalCertificateSelectionCallback { get; set; }

		public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }

		public List<SslApplicationProtocol> ApplicationProtocols { get; set; }

		public string TargetHost { get; set; }

		public X509CertificateCollection ClientCertificates { get; set; }

		public X509RevocationMode CertificateRevocationCheckMode
		{
			get
			{
				return _checkCertificateRevocation;
			}
			set
			{
				if (value != 0 && value != X509RevocationMode.Offline && value != X509RevocationMode.Online)
				{
					throw new ArgumentException();
				}
				_checkCertificateRevocation = value;
			}
		}

		public EncryptionPolicy EncryptionPolicy
		{
			get
			{
				return _encryptionPolicy;
			}
			set
			{
				if (value != 0 && value != EncryptionPolicy.AllowNoEncryption && value != EncryptionPolicy.NoEncryption)
				{
					throw new ArgumentException();
				}
				_encryptionPolicy = value;
			}
		}

		public SslProtocols EnabledSslProtocols
		{
			get
			{
				return _enabledSslProtocols;
			}
			set
			{
				_enabledSslProtocols = value;
			}
		}

		public CipherSuitesPolicy CipherSuitesPolicy { get; set; }
	}
#endif
}
