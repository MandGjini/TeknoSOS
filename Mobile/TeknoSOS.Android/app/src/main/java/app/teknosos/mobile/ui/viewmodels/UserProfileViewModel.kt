package app.teknosos.mobile.ui.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import app.teknosos.mobile.data.models.PublicUserProfile
import app.teknosos.mobile.data.models.Technician
import app.teknosos.mobile.data.repository.UserRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import javax.inject.Inject
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

@HiltViewModel
class UserProfileViewModel @Inject constructor(
    private val userRepository: UserRepository
) : ViewModel() {

    private val _publicProfile = MutableStateFlow<PublicUserProfile?>(null)
    val publicProfile: StateFlow<PublicUserProfile?> = _publicProfile.asStateFlow()

    private val _technicians = MutableStateFlow<List<Technician>>(emptyList())
    val technicians: StateFlow<List<Technician>> = _technicians.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error.asStateFlow()

    fun loadPublicProfile(id: String) {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            val result = userRepository.getPublicProfile(id)
            result.onSuccess { _publicProfile.value = it }
                .onFailure { _error.value = it.message ?: "Gabim në ngarkimin e profilit" }
            _isLoading.value = false
        }
    }

    fun loadTechnicians() {
        viewModelScope.launch {
            _isLoading.value = true
            _error.value = null
            val result = userRepository.getTechnicians()
            result.onSuccess { _technicians.value = it }
                .onFailure { _error.value = it.message ?: "Gabim në ngarkimin e teknikëve" }
            _isLoading.value = false
        }
    }
}