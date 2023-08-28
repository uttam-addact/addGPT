(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();
    
    
    // Initiate the wowjs
    new WOW().init();


    // Sticky Navbar
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.sticky-top').css('top', '0px');
        } else {
            $('.sticky-top').css('top', '-100px');
        }
    });
    
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });


    // Header carousel
    $(".header-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1500,
        items: 1,
        dots: true,
        loop: true,
        nav : true,
        navText : [
            '<i class="bi bi-chevron-left"></i>',
            '<i class="bi bi-chevron-right"></i>'
        ]
    });

    const chatlog = document.getElementById("chatlog");
    const userinput = document.getElementById("userinput");
    
    const faq = {
        "hello": "Hello! How can I assist you?",
        "how are you": "I'm just a bot, but I'm here to help!",
        "good morning": "Good morning there! hope you are doing well",
        "bye": "Goodbye! Feel free to return if you have more questions."
        // Add more questions and answers here
    };
    function sendMessage() {
        const userInput = userinput.value.trim().toLowerCase();
        if (userInput === "") return;

        // Display user message
        displayMessage(userInput, "message user-message");

        // Check if the user's message matches any predefined question
        if (faq[userInput]) {
            const botResponse = faq[userInput];
            displayMessage(botResponse, "message bot-message");
        } else {
            const defaultResponse = "I'm sorry, I don't understand that.";
            displayMessage(defaultResponse, "message bot-message");
        }

        // Clear the input field
        userinput.value = "";
    }

    function displayMessage(message, sender) {
        const messageDiv = document.createElement("div");
        messageDiv.className = sender;
        messageDiv.textContent = message;
        chatlog.appendChild(messageDiv);

        // Scroll chatbox to the bottom
        chatlog.scrollTop = chatlog.scrollHeight;
    }

    // Listen for Enter key press in the input field
    userinput.addEventListener("keyup", function(event) {
        if (event.key === "Enter") {
            sendMessage();
        }
    });
    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        center: true,
        margin: 24,
        dots: true,
        loop: true,
        nav : false,
        responsive: {
            0:{
                items:1
            },
            768:{
                items:2
            },
            992:{
                items:3
            }
        }
    });
    
})(jQuery);

