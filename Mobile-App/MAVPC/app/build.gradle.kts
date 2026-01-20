plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.google.android.libraries.mapsplatform.secrets.gradle.plugin)
}

android {
    namespace = "com.example.mavpc"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.example.mavpc"
        minSdk = 30
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    buildFeatures {
        viewBinding = true
    }
}

dependencies {
    implementation(libs.appcompat)
    implementation(libs.material)
    implementation(libs.activity)
    implementation(libs.constraintlayout)
    // estas las he añadido yo para poner radius a los botones
    implementation("com.google.android.material:material:1.11.0")
    // google maps
    implementation("com.google.android.gms:play-services-maps:18.2.0")
    // para sacar la ubicacion actual del telefono
    implementation("com.google.android.gms:play-services-location:21.0.1")
    // para llamar a la API
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    // para cargar la foto de perfil desde una URL
    implementation("com.github.bumptech.glide:glide:4.16.0")
    implementation(libs.play.services.maps)
    // para ajustar la pfp al cambiarla
    implementation("com.github.yalantis:ucrop:2.2.8")
    testImplementation(libs.junit)
    androidTestImplementation(libs.ext.junit)
    androidTestImplementation(libs.espresso.core)
}