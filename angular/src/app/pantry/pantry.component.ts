import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import {
  ConfirmationService,
  Confirmation,
  ModalComponent,
  ButtonComponent,
  ModalCloseDirective,
} from '@abp/ng.theme.shared';
import { PantryItemService } from '../proxy/pantry/pantry-item.service';
import { PantryItemDto, UpdatePantryItemDto } from '../proxy/pantry/models';

@Component({
  selector: 'app-pantry',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LocalizationPipe,
    ModalComponent,
    ButtonComponent,
    ModalCloseDirective,
  ],
  templateUrl: './pantry.component.html',
})
export class PantryComponent implements OnInit {
  private pantryService = inject(PantryItemService);
  private confirmationService = inject(ConfirmationService);
  private fb = inject(FormBuilder);

  items: PantryItemDto[] = [];
  isLoading = false;
  hasError = false;

  isModalOpen = false;
  isSaving = false;
  selectedId = '';
  form!: FormGroup;

  ngOnInit(): void {
    this.crearFormulario();
    this.cargarDatos();
  }

  crearFormulario(): void {
    this.form = this.fb.group({
      quantity: [null, [Validators.min(0)]],
      unit: ['', [Validators.maxLength(50)]],
      expirationDate: [null],
    });
  }

  cargarDatos(): void {
    this.isLoading = true;
    this.hasError = false;

    this.pantryService.getList({ skipCount: 0, maxResultCount: 100 }).subscribe({
      next: (res) => {
        this.items = res.items || [];
        this.isLoading = false;
      },
      error: () => {
        this.hasError = true;
        this.isLoading = false;
      },
    });
  }

  abrirEditar(item: PantryItemDto): void {
    this.selectedId = item.id;
    let fecha = '';
    if (item.expirationDate) {
      fecha = item.expirationDate.split('T')[0];
    }

    this.form.patchValue({
      quantity: item.quantity,
      unit: item.unit,
      expirationDate: fecha,
    });

    this.isModalOpen = true;
  }

  guardar(): void {
    if (this.form.invalid || !this.selectedId) {
      return;
    }

    this.isSaving = true;
    const datos: UpdatePantryItemDto = {
      quantity: this.form.value.quantity !== null && this.form.value.quantity !== '' ? Number(this.form.value.quantity) : null,
      unit: this.form.value.unit || null,
      expirationDate: this.form.value.expirationDate ? new Date(this.form.value.expirationDate).toISOString() : null,
    };

    this.pantryService.update(this.selectedId, datos).subscribe({
      next: () => {
        this.isSaving = false;
        this.isModalOpen = false;
        this.cargarDatos();
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }

  consumir(item: PantryItemDto): void {
    this.pantryService.consume(item.id).subscribe({
      next: () => {
        this.cargarDatos();
      },
    });
  }

  eliminar(item: PantryItemDto): void {
    this.confirmationService
      .warn('::Pantry:DeleteConfirmation', '::Pantry:AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.pantryService.delete(item.id).subscribe({
            next: () => {
              this.cargarDatos();
            },
          });
        }
      });
  }
}
