document.addEventListener("DOMContentLoaded", function () {
        const checkbox = document.getElementById("agreeCheckbox");
        const submitBtn = document.getElementById("submitBtn");

        checkbox.addEventListener("change", function () {
            submitBtn.disabled = !checkbox.checked;
        });
    });

document.addEventListener("DOMContentLoaded", function () {
    function togglePasswordVisibility(inputId, toggleId) {
        const input = document.getElementById(inputId);
        const toggleIcon = document.getElementById(toggleId);
        toggleIcon.addEventListener("click", function () {
            if (input.type === "password") {
                input.type = "text";
                toggleIcon.classList.remove("fa-eye-slash");
                toggleIcon.classList.add("fa-eye");
            } else {
                input.type = "password";
                toggleIcon.classList.remove("fa-eye");
                toggleIcon.classList.add("fa-eye-slash");
            }
        });
    }
    togglePasswordVisibility("passwordInput", "togglePassword");
    togglePasswordVisibility("confirmPasswordInput", "toggleConfirmPassword");
});