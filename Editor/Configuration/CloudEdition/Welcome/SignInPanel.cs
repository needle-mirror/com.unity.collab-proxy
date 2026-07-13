using System;

using UnityEngine.UIElements;

using Codice.Client.Common;
using Codice.Client.Common.Authentication;
using Codice.Client.Common.WebApi;
using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using PlasticGui.Configuration.CloudEdition.Welcome;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal class SignInPanel : VisualElement
    {
        internal interface IPanelHost
        {
            void ReplaceRootPanel(VisualElement panel);
        }

        internal interface IUnityIdOAuthFlowStarter
        {
            void Start(WaitingSignInPanel waitingPanel, IPlasticWebRestApi restApi);
        }

        internal SignInPanel(
            CloudEditionWelcomeWindow parentWindow, IPlasticWebRestApi restApi)
            : this(parentWindow, parentWindow, parentWindow, restApi,
                new DefaultUnityIdOAuthFlowStarter(), new DefaultExternalUrlOpener())
        {
        }

        internal SignInPanel(
            IPanelHost panelHost,
            IWelcomeWindowNotify welcomeNotify,
            OAuthSignIn.INotify oauthNotify,
            IPlasticWebRestApi restApi,
            IUnityIdOAuthFlowStarter oauthFlowStarter,
            IExternalUrlOpener urlOpener)
        {
            mPanelHost = panelHost;
            mWelcomeNotify = welcomeNotify;
            mOAuthNotify = oauthNotify;
            mRestApi = restApi;
            mOAuthFlowStarter = oauthFlowStarter;
            mUrlOpener = urlOpener;

            InitializeLayoutAndStyles();

            BuildComponents();
        }

        internal void Dispose()
        {
            mSignInWithUnityIdButton.clicked -= SignInWithUnityIdButton_Clicked;
            mSignInWithEmailButton.clicked -= SignInWithEmailButton_Clicked;
            mPrivacyPolicyStatementButton.clicked -= PrivacyPolicyStatementButton_Clicked;
            mSignUpButton.clicked -= SignUpButton_Clicked;

            if (mSignInWithEmailPanel != null)
                mSignInWithEmailPanel.Dispose();

            if (mWaitingSignInPanel != null)
                mWaitingSignInPanel.Dispose();
        }

        void SignInWithEmailButton_Clicked()
        {
            mSignInWithEmailPanel = new SignInWithEmailPanel(mWelcomeNotify, mRestApi);

            mPanelHost.ReplaceRootPanel(mSignInWithEmailPanel);
        }

        void SignUpButton_Clicked()
        {
            mUrlOpener.Open(UnityUrl.DevOps.GetSignUp());
        }

        internal void SignInWithUnityIdButton_Clicked()
        {
            mWaitingSignInPanel = new WaitingSignInPanel(
                mWelcomeNotify, mOAuthNotify, mRestApi);

            mPanelHost.ReplaceRootPanel(mWaitingSignInPanel);

            mOAuthFlowStarter.Start(mWaitingSignInPanel, mRestApi);
        }

        internal void SignInWithUnityIdButtonAutoLogin()
        {
            mWaitingSignInPanel = new WaitingSignInPanel(
                mWelcomeNotify, mOAuthNotify, mRestApi);

            mWaitingSignInPanel.OnAutoLogin();

            mPanelHost.ReplaceRootPanel(mWaitingSignInPanel);
        }

        void PrivacyPolicyStatementButton_Clicked()
        {
            mUrlOpener.Open(UnityUrl.DevOps.GetPrivacyPolicy());
        }

        void BuildComponents()
        {
            BuildSignUpArea();
            BuildSignInUnityIdArea();
            BuildSignInEmailArea();
            BuildPrivatePolicyArea();
        }

        void BuildPrivatePolicyArea()
        {
            this.SetControlText<Label>(
                "privacyStatementText",
                PlasticLocalization.Name.PrivacyStatementText,
                PlasticLocalization.GetString(PlasticLocalization.Name.PrivacyStatement));

            mPrivacyPolicyStatementButton = this.Query<Button>(PRIVACY_BUTTON);
            mPrivacyPolicyStatementButton.text = PlasticLocalization.Name.PrivacyStatement.GetString();
            mPrivacyPolicyStatementButton.clicked += PrivacyPolicyStatementButton_Clicked;
        }

        void BuildSignInEmailArea()
        {
            this.SetControlImage(
                "iconEmail",
                Images.Name.ButtonSsoSignInEmail);

            mSignInWithEmailButton = this.Query<Button>(EMAIL_BUTTON);
            mSignInWithEmailButton.text = PlasticLocalization.Name.SignInWithEmail.GetString();
            mSignInWithEmailButton.clicked += SignInWithEmailButton_Clicked;
        }

        void BuildSignInUnityIdArea()
        {
            this.SetControlImage(
                "iconUnity",
                Images.Name.ButtonSsoSignInUnity);

            mSignInWithUnityIdButton = this.Query<Button>(UNITY_ID_BUTTON);
            mSignInWithUnityIdButton.text = PlasticLocalization.Name.SignInWithUnityID.GetString();
            mSignInWithUnityIdButton.clicked += SignInWithUnityIdButton_Clicked;
        }

        void BuildSignUpArea()
        {
            Label signUpLabel = this.Query<Label>("signUpLabel");
            signUpLabel.text = PlasticLocalization.Name.LoginOrSignUp.GetString();

            mSignUpButton = this.Query<Button>(SIGN_UP_BUTTON);
            mSignUpButton.text = PlasticLocalization.Name.SignUpButton.GetString();
            mSignUpButton.clicked += SignUpButton_Clicked;
        }

        void InitializeLayoutAndStyles()
        {
            AddToClassList("grow");

            this.LoadLayout(typeof(SignInPanel).Name);
            this.LoadStyle(typeof(SignInPanel).Name);
        }

        class DefaultUnityIdOAuthFlowStarter : IUnityIdOAuthFlowStarter
        {
            void IUnityIdOAuthFlowStarter.Start(
                WaitingSignInPanel waitingPanel, IPlasticWebRestApi restApi)
            {
                Guid state = Guid.NewGuid();

                waitingPanel.OAuthSignIn(
                    GetAuthProviders.GetUnityIdAuthProvider(string.Empty, state),
                    GetCredentialsFromState.Build(
                        string.Empty, state, SEIDWorkingMode.SSOWorkingMode, restApi));
            }
        }

        SignInWithEmailPanel mSignInWithEmailPanel;
        WaitingSignInPanel mWaitingSignInPanel;
        Button mSignInWithUnityIdButton;
        Button mSignInWithEmailButton;
        Button mPrivacyPolicyStatementButton;
        Button mSignUpButton;

        readonly IPanelHost mPanelHost;
        readonly IWelcomeWindowNotify mWelcomeNotify;
        readonly OAuthSignIn.INotify mOAuthNotify;
        readonly IPlasticWebRestApi mRestApi;
        readonly IUnityIdOAuthFlowStarter mOAuthFlowStarter;
        readonly IExternalUrlOpener mUrlOpener;

        internal const string UNITY_ID_BUTTON = "unityIDButton";
        internal const string EMAIL_BUTTON = "emailButton";
        internal const string SIGN_UP_BUTTON = "signUpButton";
        internal const string PRIVACY_BUTTON = "privacyStatement";
    }
}
