package app.teknosos.mobile.ui.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import app.teknosos.mobile.data.models.User
import app.teknosos.mobile.data.repository.AuthRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * Authentication ViewModel
 */
@HiltViewModel
class AuthViewModel @Inject constructor(
    private val authRepository: AuthRepository
) : ViewModel() {

    private val _isAuthenticated = MutableStateFlow(false)
    val isAuthenticated: StateFlow<Boolean> = _isAuthenticated.asStateFlow()

    private val _currentUser = MutableStateFlow<User?>(null)
    val currentUser: StateFlow<User?> = _currentUser.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    init {
        checkAuthState()
    }

    private fun checkAuthState() {
        viewModelScope.launch {
            _isLoading.value = true
            try {
                val isValid = authRepository.checkAuthState()
                _isAuthenticated.value = isValid
                if (isValid) {
                    _currentUser.value = authRepository.getCurrentUser()
                }
            } catch (e: Exception) {
                _isAuthenticated.value = false
            } finally {
                _isLoading.value = false
            }
        }
    }

    fun login(email: String, password: String) {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            try {
                val result = authRepository.login(email, password)
                if (result.isSuccess) {
                    _currentUser.value = result.getOrNull()
                    _isAuthenticated.value = true
                } else {
                    _error.value = result.exceptionOrNull()?.message ?: "Gabim në hyrje"
                }
            } catch (e: Exception) {
                _error.value = e.message ?: "Gabim në hyrje"
            } finally {
                _isLoading.value = false
            }
        }
    }

    fun register(
        email: String,
        password: String,
        firstName: String,
        lastName: String,
        phoneNumber: String?,
        isProfessional: Boolean
    ) {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            try {
                val result = authRepository.register(
                    email, password, firstName, lastName, phoneNumber, isProfessional
                )
                if (result.isSuccess) {
                    _currentUser.value = result.getOrNull()
                    _isAuthenticated.value = true
                } else {
                    _error.value = result.exceptionOrNull()?.message ?: "Gabim në regjistrim"
                }
            } catch (e: Exception) {
                _error.value = e.message ?: "Gabim në regjistrim"
            } finally {
                _isLoading.value = false
            }
        }
    }

    fun logout() {
        viewModelScope.launch {
            authRepository.logout()
            _currentUser.value = null
            _isAuthenticated.value = false
        }
    }

    fun clearError() {
        _error.value = null
    }
}
