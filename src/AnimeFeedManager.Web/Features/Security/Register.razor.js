﻿function registerUser() {
    return {
        userId: '',
        valid: true,
        result: {success: true, showSuccess: false, errors: {}},
        async register(userId) {
            const Client = Passwordless.Client;
            const p = new Client({
                apiKey: API_KEY
            });

            // Fetch the registration token from the backend.
            try {

                const registerTokenResult = await fetch('/create-token?alias=' + userId).then((r) =>
                    r.json()
                );

                if (registerTokenResult.status && registerTokenResult.status >= 400) {
                    this.result = {
                        errors: registerTokenResult.errors ?? {fail: registerTokenResult.title},
                        showSuccess: false,
                        success: false
                    }
                    return;
                }

                // Register the token with the end-user's device.
                const {token, error} = await p.register(registerTokenResult.token);

                if (token) {
                    this.result = {
                        success: true,
                        showSuccess: true
                    }
                } else {
                    this.result = {
                        errors: error,
                        success: false,
                        showSuccess: false,
                    }
                    this.valid = false;
                }

            } catch (e) {
                console.error("Things went bad on registration", e);
                this.result = {
                    errors: {ex: [e]},
                    success: false
                }

            }
        }
    }
}