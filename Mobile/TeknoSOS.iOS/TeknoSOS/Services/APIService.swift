//
//  APIService.swift
//  TeknoSOS
//
//  HTTP client for TeknoSOS API
//

import Foundation

actor APIService {
    static let shared = APIService()
    
    #if DEBUG
    private let baseURL = "http://localhost:5050/api/v1"
    #else
    private let baseURL = "https://teknosos.app/api/v1"
    #endif
    
    private let session: URLSession
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder
    
    private init() {
        let config = URLSessionConfiguration.default
        config.timeoutIntervalForRequest = 30
        config.timeoutIntervalForResource = 60
        session = URLSession(configuration: config)
        
        decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        
        encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
    }
    
    // MARK: - Auth Endpoints
    func login(request: LoginRequest) async throws -> APIResponse<AuthResponse> {
        try await post("/auth/login", body: request)
    }
    
    func register(request: RegisterRequest) async throws -> APIResponse<AuthResponse> {
        try await post("/auth/register", body: request)
    }
    
    func refreshToken(accessToken: String, refreshToken: String) async throws -> AuthResponse {
        let request = RefreshTokenRequest(accessToken: accessToken, refreshToken: refreshToken)
        let response: APIResponse<AuthResponse> = try await post("/auth/refresh", body: request)
        
        guard let data = response.data else {
            throw APIError.invalidResponse
        }
        return data
    }
    
    func logout() async throws -> APIResponse<EmptyResponse> {
        try await post("/auth/logout", body: EmptyRequest())
    }
    
    func getProfile() async throws -> APIResponse<User> {
        try await get("/auth/profile")
    }

    func getPublicProfile(id: String) async throws -> APIResponse<PublicUserProfile> {
        try await get("/auth/profile/\(id)")
    }

    // MARK: - Defects Endpoints
    func getMyDefects(page: Int = 1, status: String? = nil) async throws -> APIResponse<PagedResult<DefectSummary>> {
        var queryItems = [URLQueryItem(name: "page", value: "\(page)")]
        if let status = status {
            queryItems.append(URLQueryItem(name: "status", value: status))
        }
        return try await get("/defects", queryItems: queryItems)
    }
    
    func getDefect(id: Int) async throws -> APIResponse<Defect> {
        try await get("/defects/\(id)")
    }
    
    func createDefect(request: CreateDefectRequest) async throws -> APIResponse<Defect> {
        try await post("/defects", body: request)
    }
    
    func getNearbyDefects(lat: Double, lng: Double, radiusKm: Double = 10) async throws -> APIResponse<[DefectSummary]> {
        let queryItems = [
            URLQueryItem(name: "lat", value: "\(lat)"),
            URLQueryItem(name: "lng", value: "\(lng)"),
            URLQueryItem(name: "radiusKm", value: "\(radiusKm)")
        ]
        return try await get("/defects/nearby", queryItems: queryItems)
    }
    
    // MARK: - Technicians Endpoints
    func getTechnicians(category: String? = nil, lat: Double? = nil, lng: Double? = nil) async throws -> APIResponse<[Technician]> {
        var queryItems = [URLQueryItem]()
        if let category = category {
            queryItems.append(URLQueryItem(name: "category", value: category))
        }
        if let lat = lat, let lng = lng {
            queryItems.append(URLQueryItem(name: "lat", value: "\(lat)"))
            queryItems.append(URLQueryItem(name: "lng", value: "\(lng)"))
        }
        return try await get("/technicians", queryItems: queryItems)
    }
    
    // MARK: - HTTP Methods
    private func get<T: Decodable>(_ path: String, queryItems: [URLQueryItem] = []) async throws -> T {
        var components = URLComponents(string: baseURL + path)!
        if !queryItems.isEmpty {
            components.queryItems = queryItems
        }
        
        var request = URLRequest(url: components.url!)
        request.httpMethod = "GET"
        request = await addHeaders(to: request)
        
        return try await execute(request)
    }
    
    private func post<T: Decodable, B: Encodable>(_ path: String, body: B) async throws -> T {
        var request = URLRequest(url: URL(string: baseURL + path)!)
        request.httpMethod = "POST"
        request.httpBody = try encoder.encode(body)
        request = await addHeaders(to: request)
        
        return try await execute(request)
    }
    
    private func put<T: Decodable, B: Encodable>(_ path: String, body: B) async throws -> T {
        var request = URLRequest(url: URL(string: baseURL + path)!)
        request.httpMethod = "PUT"
        request.httpBody = try encoder.encode(body)
        request = await addHeaders(to: request)
        
        return try await execute(request)
    }
    
    private func delete<T: Decodable>(_ path: String) async throws -> T {
        var request = URLRequest(url: URL(string: baseURL + path)!)
        request.httpMethod = "DELETE"
        request = await addHeaders(to: request)
        
        return try await execute(request)
    }
    
    // MARK: - Helpers
    private func addHeaders(to request: URLRequest) async -> URLRequest {
        var request = request
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue("application/json", forHTTPHeaderField: "Accept")
        request.setValue("TeknoSOS-iOS/1.0", forHTTPHeaderField: "User-Agent")

        let token = await MainActor.run {
            AuthenticationManager.shared.accessToken
        }

        if let token {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }

        return request
    }
    
    private func execute<T: Decodable>(_ request: URLRequest) async throws -> T {
        let (data, response) = try await session.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse else {
            throw APIError.invalidResponse
        }
        
        switch httpResponse.statusCode {
        case 200...299:
            return try decoder.decode(T.self, from: data)
        case 401:
            throw APIError.unauthorized
        case 403:
            throw APIError.forbidden
        case 404:
            throw APIError.notFound
        case 500...599:
            throw APIError.serverError
        default:
            // Try to parse error message
            if let errorResponse = try? decoder.decode(APIResponse<EmptyResponse>.self, from: data) {
                throw APIError.custom(errorResponse.message ?? "Unknown error")
            }
            throw APIError.unknown
        }
    }
}

// MARK: - API Errors
enum APIError: LocalizedError {
    case invalidResponse
    case unauthorized
    case forbidden
    case notFound
    case serverError
    case networkError
    case custom(String)
    case unknown
    
    var errorDescription: String? {
        switch self {
        case .invalidResponse:
            return "Përgjigje e pavlefshme nga serveri"
        case .unauthorized:
            return "Sesioni ka skaduar"
        case .forbidden:
            return "Nuk keni leje"
        case .notFound:
            return "Nuk u gjet"
        case .serverError:
            return "Gabim në server"
        case .networkError:
            return "Gabim në lidhje"
        case .custom(let message):
            return message
        case .unknown:
            return "Gabim i panjohur"
        }
    }
}

// MARK: - Empty Types
struct EmptyRequest: Encodable {}
struct EmptyResponse: Decodable {}
