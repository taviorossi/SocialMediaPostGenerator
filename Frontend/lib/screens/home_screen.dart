import 'dart:async';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

import '../config/api_config.dart';
import '../models/generate_video_response.dart';
import '../models/video_job_status_response.dart';
import '../services/video_api_service.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final VideoApiService _api = VideoApiService(baseUrl: kBackendBaseUrl);
  String? _jobId;
  VideoJobStatusResponse? _status;
  String? _error;
  bool _loading = false;
  Timer? _pollTimer;

  @override
  void dispose() {
    _pollTimer?.cancel();
    super.dispose();
  }

  Future<void> _pickAndUpload() async {
    setState(() {
      _error = null;
      _status = null;
      _jobId = null;
      _loading = true;
    });
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.image,
        allowMultiple: false,
      );
      if (result == null || result.files.isEmpty || result.files.single.path == null) {
        setState(() => _loading = false);
        return;
      }
      final path = result.files.single.path!;
      final response = await _api.uploadImage(path);
      setState(() {
        _jobId = response.jobId;
        _loading = false;
        _status = VideoJobStatusResponse(
          jobId: response.jobId,
          status: response.status,
        );
      });
      _startPolling();
    } catch (e, st) {
      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  void _startPolling() {
    _pollTimer?.cancel();
    _pollTimer = Timer.periodic(const Duration(seconds: 2), (_) async {
      if (_jobId == null) return;
      try {
        final status = await _api.getJobStatus(_jobId!);
        if (!mounted) return;
        setState(() => _status = status);
        if (status.status == 'Done' || status.status == 'Failed') {
          _pollTimer?.cancel();
        }
      } catch (_) {}
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Violeta & Cacau - Geração de Vídeos'),
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      ),
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const SizedBox(height: 16),
            ElevatedButton.icon(
              onPressed: _loading ? null : _pickAndUpload,
              icon: _loading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.upload_file),
              label: Text(_loading ? 'Enviando...' : 'Enviar imagem'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
            ),
            const SizedBox(height: 24),
            if (_error != null)
              Card(
                color: Theme.of(context).colorScheme.errorContainer,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Text(_error!, style: TextStyle(color: Theme.of(context).colorScheme.onErrorContainer)),
                ),
              ),
            if (_status != null) ...[
              const SizedBox(height: 16),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Status do processamento',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 12),
                      _StatusRow(label: 'Job ID', value: _status!.jobId),
                      _StatusRow(label: 'Etapa', value: _status!.statusLabel),
                      if (_status!.errorMessage != null)
                        _StatusRow(label: 'Erro', value: _status!.errorMessage!),
                      if (_status!.videoUrl != null) ...[
                        const SizedBox(height: 12),
                        const Text('Vídeo pronto:'),
                        const SizedBox(height: 4),
                        SelectableText(
                          _status!.videoUrl!,
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                        const SizedBox(height: 8),
                        OutlinedButton.icon(
                          onPressed: () {
                            final uri = Uri.tryParse(_status!.videoUrl!);
                            if (uri != null) launchUrl(uri, mode: LaunchMode.externalApplication);
                          },
                          icon: const Icon(Icons.open_in_new),
                          label: const Text('Abrir vídeo'),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _StatusRow extends StatelessWidget {
  const _StatusRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(label, style: const TextStyle(fontWeight: FontWeight.w500)),
          ),
          Expanded(child: SelectableText(value)),
        ],
      ),
    );
  }
}
