using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mail.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyApplicationComponent.Tests
{
    [TestClass]
    public class SendEmailTests
    {
        [TestMethod]
        public void WhenSmtpClientSendRequestedThenMailMessageValuesArePopulatedCorrectly()
        {
            using (ShimsContext.Create())
            {
                var emailInfo =
                    new EmailInformation
                    {
                        FromAddress = "from@mail.com",
                        FromName = "From Name",
                        ToAddress = "to@mail.com",
                        ToName = "To Name",
                        Subject = "Email Subject",
                        MessageText = "Email Body",
                        IsHtmlMessage = false,
                    };

                int mailAddrCalled = 0;
                ShimMailAddress.ConstructorStringString =
                    (@this, addr, name) =>
                    {
                        new ShimMailAddress(@this);
                        switch (mailAddrCalled++)
                        {
                            case 0:
                                Assert.AreEqual("from@mail.com", addr);
                                Assert.AreEqual("From Name", name);
                                break;
                            case 1:
                                Assert.AreEqual("to@mail.com", addr);
                                Assert.AreEqual("To Name", name);
                                break;
                        }
                    };

                ShimSmtpClient.Constructor =
                    @this =>
                    {
                        var shim = new ShimSmtpClient(@this);
                        shim.SendMailMessage = e => { };
                    };

                int fromCalled = 0;
                int toCalled = 0;
                int subjectCalled = 0;
                int bodyCalled = 0;
                int isHtmlCalled = 0;
                ShimMailMessage.Constructor =
                    @this =>
                    {
                        var msg = new ShimMailMessage(@this);
                        msg.FromSetMailAddress =
                            mailAddr =>
                            {
                                ++fromCalled;
                            };
                        msg.ToGet =
                            () =>
                            {
                                ++toCalled;
                                return new MailAddressCollection();
                            };
                        msg.SubjectSetString =
                            subject =>
                            {
                                Assert.AreEqual("Email Subject", subject);
                                ++subjectCalled;
                            };
                        msg.BodySetString =
                            bodyText =>
                            {
                                Assert.AreEqual("Email Body", bodyText);
                                ++bodyCalled;
                            };
                        msg.IsBodyHtmlSetBoolean =
                            isHtml =>
                            {
                                Assert.AreEqual(false, isHtml);
                                ++isHtmlCalled;
                            };
                    };

                var sendEmail = new SendEmail();
                sendEmail.Send(emailInfo);

                Assert.AreEqual(2, mailAddrCalled);
                Assert.AreEqual(1, fromCalled);
                Assert.AreEqual(1, toCalled);
                Assert.AreEqual(1, subjectCalled);
                Assert.AreEqual(1, bodyCalled);
                Assert.AreEqual(1, isHtmlCalled);
            }
        }

        [TestMethod]
        public void WhenSmtpClientSuccessfullySendsThenSuccessStatusReturned()
        {
            using (ShimsContext.Create())
            {
                var emailInfo = new EmailInformation { };

                ShimMailAddress.ConstructorStringString =
                    (@this, addr, name) =>
                    {
                        new ShimMailAddress(@this);
                    };

                int emailSendCalled = 0;
                ShimSmtpClient.Constructor =
                    @this =>
                    {
                        var shim = new ShimSmtpClient(@this);
                        shim.SendMailMessage =
                            e =>
                            {
                                ++emailSendCalled;
                            };
                    };

                ShimMailMessage.Constructor =
                    @this =>
                    {
                        var msg = new ShimMailMessage(@this);
                        msg.FromSetMailAddress = mailAddr => { };
                        msg.ToGet = () => new MailAddressCollection();
                        msg.SubjectSetString = subject => { };
                        msg.BodySetString = bodyText => { };
                        msg.IsBodyHtmlSetBoolean = isHtml => { };
                    };

                var sendEmail = new SendEmail();
                var status = sendEmail.Send(emailInfo);

                Assert.AreEqual(1, emailSendCalled);

                Assert.IsNotNull(status);
                Assert.AreEqual(true, status.WasSent);
                Assert.IsNull(status.ErrorMessage);
            }
        }

        [TestMethod]
        public void WhenSmtpClientFailsToSendThenErrorStatusReturned()
        {
            using (ShimsContext.Create())
            {
                var emailInfo = new EmailInformation { };

                ShimMailAddress.ConstructorStringString =
                    (@this, addr, name) =>
                    {
                        new ShimMailAddress(@this);
                    };

                int emailSendCalled = 0;
                ShimSmtpClient.Constructor =
                    @this =>
                    {
                        var shim = new ShimSmtpClient(@this);
                        shim.SendMailMessage =
                            e =>
                            {
                                ++emailSendCalled;
                                throw new SmtpException("Error sending email.");
                            };
                    };

                ShimMailMessage.Constructor =
                    @this =>
                    {
                        var msg = new ShimMailMessage(@this);
                        msg.FromSetMailAddress = mailAddr => { };
                        msg.ToGet = () => new MailAddressCollection();
                        msg.SubjectSetString = subject => { };
                        msg.BodySetString = bodyText => { };
                        msg.IsBodyHtmlSetBoolean = isHtml => { };
                    };

                var sendEmail = new SendEmail();
                var status = sendEmail.Send(emailInfo);

                Assert.AreEqual(1, emailSendCalled);

                Assert.IsNotNull(status);
                Assert.AreEqual(false, status.WasSent);
                Assert.AreEqual("Error sending email.", status.ErrorMessage);
            }
        }
    }
}
